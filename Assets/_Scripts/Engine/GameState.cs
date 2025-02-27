using System;
using System.Collections;
using System.Collections.Generic;

public enum MillGameStage
{
    PlacingPawns,
    NormalPlay
}

public class GameState
{
    private static int N_POSSIBLE_MILLS = 16;
    private static int PLAYERS_PAWNS = 9;
    private static int LOSING_PAWNS_NUMBER_THRESHOLD = 2;
    private static int FLYING_PAWNS_NUMBER = 3;
    private static Mill[] POSSIBLE_MILLS;
    private static List<List<int>> possibleMoveIndices;
    private static string[] fieldNames;
    private static Dictionary<PlayerNumber, string> playerNames;

    public string MovesUntilNow { get; private set; }

    public delegate void GameStateChanged();
    public event GameStateChanged OnGameStateChanged = delegate { };

    public delegate void LastSelectedFieldChanged();
    public event LastSelectedFieldChanged OnLastSelectedFieldChanged = delegate { };

    public MillGameStage GameStage { get; private set; }
    public int FirstPlayersPawnsLeft {
        get {
            return CurrentBoard.GetPlayerFields(PlayerNumber.FirstPlayer).Count;
        }
    }
    public int SecondPlayersPawnsLeft
    {
        get
        {
            return CurrentBoard.GetPlayerFields(PlayerNumber.SecondPlayer).Count;
        }
    }
    public int FirstPlayersPawnsToPlaceLeft { get; private set; }
    public int SecondPlayersPawnsToPlaceLeft { get; private set; }
    public PlayerNumber WinningPlayer { get; private set; }
    public PlayerNumber CurrentMovingPlayer { get; private set; }
    public PlayerNumber OtherPlayer
    {
        get
        {
            return CurrentMovingPlayer == PlayerNumber.FirstPlayer ? PlayerNumber.SecondPlayer : PlayerNumber.FirstPlayer;
        }
    }
    public int CurrentPlayersPawnsLeft
    {
        get
        {
            if(CurrentMovingPlayer == PlayerNumber.FirstPlayer)
            {
                return FirstPlayersPawnsLeft;
            } else
            {
                return SecondPlayersPawnsLeft;
            }
        }
    }

    public HashSet<Mill> ActiveMills { get; private set; }
    public HashSet<Mill> ClosedMills { get; private set; }
    public int PawnsToRemove { get; set; }

    private Field _lastSelectedField = null;
    public Field LastSelectedField {
        get
        {
            return _lastSelectedField;
        }
        set
        {
            _lastSelectedField = value;
            OnLastSelectedFieldChanged();
        }
    }

    public double Evaluation { get; set; }

    public Board CurrentBoard { get; }

    public int MovesMade { get; private set; }

    public bool GameFinished {
        get
        {
            return WinningPlayer != PlayerNumber.None;
        }
    }

    public GameState()
    {
        CurrentBoard = new Board();
        GameStage = MillGameStage.PlacingPawns;
        FirstPlayersPawnsToPlaceLeft = PLAYERS_PAWNS;
        SecondPlayersPawnsToPlaceLeft = PLAYERS_PAWNS;
        WinningPlayer = PlayerNumber.None;
        CurrentMovingPlayer = PlayerNumber.FirstPlayer;
        ActiveMills = new HashSet<Mill>();
        ClosedMills = new HashSet<Mill>();
        PawnsToRemove = 0;
        MovesMade = 0;
        MovesUntilNow = "";
    }
    
    public GameState(GameState other)
    {
        CurrentBoard = new Board(other.CurrentBoard);
        GameStage = other.GameStage;
        FirstPlayersPawnsToPlaceLeft = other.FirstPlayersPawnsToPlaceLeft;
        SecondPlayersPawnsToPlaceLeft = other.SecondPlayersPawnsToPlaceLeft;
        WinningPlayer = other.WinningPlayer;
        CurrentMovingPlayer = other.CurrentMovingPlayer;
        ActiveMills = new HashSet<Mill>(other.ActiveMills);
        ClosedMills = new HashSet<Mill>(other.ClosedMills);
        PawnsToRemove = other.PawnsToRemove;
        LastSelectedField = null;
        MovesMade = other.MovesMade;
        MovesUntilNow = string.Copy(other.MovesUntilNow);
    }
    public void HandleSelection(int fieldIndex)
    {
        if (PawnsToRemove > 0)
        {
            HandlePawnRemoval(fieldIndex);
        }
        else if (GameStage == MillGameStage.PlacingPawns)
        {
            HandlePawnPlacing(fieldIndex);
        }
        else
        {
            HandlePawnMoving(fieldIndex);
        }
        CheckGameStateChanged();
        OnGameStateChanged();
    }

    private void CheckGameStateChanged()
    {
        if (GameStage == MillGameStage.PlacingPawns)
        {
            if (FirstPlayersPawnsToPlaceLeft == 0 && SecondPlayersPawnsToPlaceLeft == 0)
            {
                GameStage = MillGameStage.NormalPlay;
                CheckGameStateChanged();
            }
        }
        else
        {
            RecalculateWinningPlayer();
        }
    }

    private void HandlePawnMoving(int fieldIndex)
    {
        Field newField = CurrentBoard.GetField(fieldIndex);
        PlayerNumber selectedFieldPawnPlayer = newField.PawnPlayerNumber;
        if (selectedFieldPawnPlayer == CurrentMovingPlayer)
        {
            LastSelectedField = newField;
        }
        else if (LastSelectedField != null && newField.Empty)
        {
            if (CurrentPlayersPawnsLeft <= FLYING_PAWNS_NUMBER)
            {
                HandleFlyingMove(newField);
            }
            else
            {
                HandleNormalMove(newField);
            }
        }
    }

    private void HandleNormalMove(Field newField)
    {
        List<Field> possibleNewFields = GetPossibleNewFields(LastSelectedField.FieldIndex, CurrentBoard);
        if (possibleNewFields.Contains(newField))
        {
            PerformSelectedMove(newField);
        }
    }

    private void HandleFlyingMove(Field newField)
    {
        if (LastSelectedField.CanMoveTo(newField))
        {
            PerformSelectedMove(newField);
        }
    }

    private void PerformSelectedMove(Field newField)
    {
        LogMoveMove(CurrentMovingPlayer, LastSelectedField.FieldIndex, newField.FieldIndex);
        LastSelectedField.MoveTo(newField);
        LastSelectedField = null;
        MovesMade++;
        TogglePawnDeletingOrSwitchPlayer();
    }

    private void PerformMove(Move move)
    {
        LogMoveMove(CurrentMovingPlayer, move.FromFieldIndex, move.ToFieldIndex);
        CurrentBoard.GetField(move.FromFieldIndex).MoveTo(CurrentBoard.GetField(move.ToFieldIndex));
        MovesMade++;
        TogglePawnDeletingOrSwitchPlayer();
    }

    public void HandlePawnRemoval(int fieldIndex)
    {
        Field field = CurrentBoard.GetField(fieldIndex);
        if (!field.Empty)
        {
            if (!field.BelongsTo(CurrentMovingPlayer))
            {
                RemovePawn(field);
            }
        }
    }

    private void RemovePawn(int index)
    {
        CurrentBoard.GetField(index).Reset();
    }

    private void RemovePawn(Field field)
    {
        field.Reset();
        LogRemoveMove(CurrentMovingPlayer, field.FieldIndex);
        PawnsToRemove--;
        MovesMade++;
        RecalculateActiveMills();
        if (PawnsToRemove == 0)
        {
            SwitchPlayer();
        }
    }

    private void HandlePawnPlacing(int fieldIndex)
    {
        Field selectedField = CurrentBoard.GetField(fieldIndex);
        if (selectedField.Empty)
        {
            CurrentPlayerPlacePawn(fieldIndex);
            TogglePawnDeletingOrSwitchPlayer();
        }
    }

    private void CurrentPlayerPlacePawn(int fieldIndex)
    {
        PlayerPlacePawn(fieldIndex, CurrentMovingPlayer);
    }

    private void PlayerPlacePawn(int fieldIndex, PlayerNumber playerNumber)
    {
        CurrentBoard.Fields[fieldIndex].PawnPlayerNumber = playerNumber;
        NotePlayerPawnPlacing(playerNumber);
        LogPlaceMove(playerNumber, fieldIndex);
        MovesMade++;
    }

    private void TogglePawnDeletingOrSwitchPlayer()
    {
        MillDifference millDifference = GetMillDifference(ActiveMills, CurrentBoard);
        if (millDifference.NewMills.Count > 0)
        {
            PawnsToRemove = millDifference.NewMills.Count;
            ActiveMills = millDifference.TurnActiveMills;
            ClosedMills = millDifference.NewMills;
        }
        else
        {
            SwitchPlayer();
        }
    }

    private void NoteCurrentPlayerPawnPlacing()
    {
        NotePlayerPawnPlacing(CurrentMovingPlayer);
    }

    private void NotePlayerPawnPlacing(PlayerNumber playerNumber)
    {
        if (playerNumber == PlayerNumber.FirstPlayer)
        {
            FirstPlayersPawnsToPlaceLeft--;
        }
        else
        {
            SecondPlayersPawnsToPlaceLeft--;
        }
    }

    private void SwitchPlayer()
    {
        if (CurrentMovingPlayer == PlayerNumber.FirstPlayer)
        {
            CurrentMovingPlayer = PlayerNumber.SecondPlayer;
        }
        else
        {
            CurrentMovingPlayer = PlayerNumber.FirstPlayer;
        }
    }

    private void RecalculateActiveMills()
    {
        HashSet<Mill> newMills = GetActiveMills(CurrentBoard);
        HashSet<Mill> closedMills = new HashSet<Mill>(newMills);
        closedMills.ExceptWith(ActiveMills);
        ActiveMills = newMills;
        ClosedMills = closedMills;
    }
    private void RecalculateWinningPlayer()
    {
        if(GameStage == MillGameStage.PlacingPawns)
        {
            WinningPlayer = PlayerNumber.None;
        }
        else
        {
            if(PawnsToRemove <= 0)
            {

                int firstPlayersPossibleMoves = GetAllPossibleMoves(PlayerNumber.FirstPlayer, CurrentBoard).Count;
                int secondPlayerPossibleMoves = GetAllPossibleMoves(PlayerNumber.SecondPlayer, CurrentBoard).Count;
                if (FirstPlayersPawnsLeft <= LOSING_PAWNS_NUMBER_THRESHOLD || firstPlayersPossibleMoves == 0)
                {
                    WinningPlayer = PlayerNumber.SecondPlayer;
                }
                else if (SecondPlayersPawnsLeft <= LOSING_PAWNS_NUMBER_THRESHOLD || secondPlayerPossibleMoves == 0)
                {
                    WinningPlayer = PlayerNumber.FirstPlayer;
                }
            }
        }
    }

    private GameState PawnPlacingGameStateFromThis(PlayerNumber playerNumber, int fieldIndex)
    {
        GameState newState = new GameState(this);
        newState.PlayerPlacePawn(fieldIndex, playerNumber);
        newState.SwitchPlayer();
        newState.RecalculateActiveMills();
        newState.CheckGameStateChanged();
        return newState;
    }

    private GameState PawnRemovalGameStateFromThis(int fieldIndex)
    {
        GameState newState = new GameState(this);
        newState.RemovePawn(newState.CurrentBoard.GetField(fieldIndex));
        newState.RecalculateActiveMills();
        newState.CheckGameStateChanged();
        return newState;
    }

    private void PawnMoveGameStatesFromThis(Move move, PlayerNumber playerNumber, List<GameState> listOfAll)
    {
        PlayerNumber otherPlayerNumber = playerNumber == PlayerNumber.FirstPlayer ? PlayerNumber.SecondPlayer : PlayerNumber.FirstPlayer;
        GameState newState = new GameState(this);
        newState.PerformMove(move);
        if(newState.PawnsToRemove == 0)
        {
            newState.CheckGameStateChanged();
            listOfAll.Add(newState);
        }
        else
        {
            List<Field> otherPlayersFields = newState.CurrentBoard.GetPlayerFields(otherPlayerNumber);
            foreach (var otherPlayerField in otherPlayersFields)
            {
                listOfAll.Add(newState.PawnRemovalGameStateFromThis(otherPlayerField.FieldIndex));
            }
        }
    }

    private List<GameState> GetFirstStageNextPossibleStates(PlayerNumber playerNumber)
    {
        PlayerNumber otherPlayerNumber = playerNumber == PlayerNumber.FirstPlayer ? PlayerNumber.SecondPlayer : PlayerNumber.FirstPlayer;
        List<GameState> gameStates = new List<GameState>();
        List<Field> emptyFields = CurrentBoard.GetEmptyFields();
        foreach (var emptyField in emptyFields)
        {
            GameState nextGameState = PawnPlacingGameStateFromThis(playerNumber, emptyField.FieldIndex);
            HashSet<Mill> millDifference = nextGameState.MillDifference(this);
            if (millDifference.Count == 0)
            {
                gameStates.Add(nextGameState);
            }
            else if (millDifference.Count == 1)
            {
                List<Field> otherPlayersFields = nextGameState.CurrentBoard.GetPlayerFields(otherPlayerNumber);
                foreach (var otherPlayerField in otherPlayersFields)
                {
                    gameStates.Add(nextGameState.PawnRemovalGameStateFromThis(otherPlayerField.FieldIndex));
                }
            }
            else
            {
                List<Field> otherPlayersFields = nextGameState.CurrentBoard.GetPlayerFields(otherPlayerNumber);
                for (int i = 0; i < otherPlayersFields.Count - 1; i++)
                {
                    GameState firstRemovedPawnGameState = nextGameState.PawnRemovalGameStateFromThis(otherPlayersFields[i].FieldIndex);
                    for (int j = i + 1; j < otherPlayersFields.Count; j++)
                    {
                        GameState secondRemovedPawnGameState = firstRemovedPawnGameState.PawnRemovalGameStateFromThis(otherPlayersFields[j].FieldIndex);
                    }
                }
            }
        }
        return gameStates;
    }

    private List<GameState> GetSecondStageNextPossibleStates(PlayerNumber playerNumber)
    {
        PlayerNumber otherPlayerNumber = playerNumber == PlayerNumber.FirstPlayer ? PlayerNumber.SecondPlayer : PlayerNumber.FirstPlayer;
        List<GameState> gameStates = new List<GameState>();
        List<Move> possibleMoves = GetAllPossibleMoves(playerNumber, CurrentBoard);
        foreach (var move in possibleMoves)
        {
            PawnMoveGameStatesFromThis(move, playerNumber, gameStates);
        }
        return gameStates;
    }

    public List<GameState> GetAllPossibleNextStates(PlayerNumber playerNumber)
    {
        if (GameStage == MillGameStage.PlacingPawns)
        {
            return GetFirstStageNextPossibleStates(playerNumber);
        }
        else
        {
            return GetSecondStageNextPossibleStates(playerNumber);
        }
    }

    public int GetPossibleMovesNumberForPlayer(PlayerNumber playerNumber)
    {
        return GetAllPossibleMoves(playerNumber, CurrentBoard).Count;
    }

    private List<Move> GetAllPossibleMoves(PlayerNumber playerNumber, Board board)
    {
        List<Move> allMoves = new List<Move>();
        List<Field> playersFields = board.GetPlayerFields(playerNumber);
        int playerPawns = playersFields.Count;
        if (playerPawns <= PlayerData.FLYING_PAWNS_NUMBER)
        {
            foreach (var fromField in playersFields)
            {
                List<Field> toFields = GetPossibleNewFieldsFlying(fromField, board);
                foreach (var toField in toFields)
                {
                    allMoves.Add(new Move(fromField.FieldIndex, toField.FieldIndex));
                }
            }
        }
        else
        {
            foreach (var fromField in playersFields)
            {
                List<Field> toFields = GetPossibleNewFields(fromField, board);
                foreach (var toField in toFields)
                {
                    allMoves.Add(new Move(fromField.FieldIndex, toField.FieldIndex));
                }
            }
        }

        return allMoves;
    }

    private List<Field> GetPossibleNewFields(int fromIndex, Board board)
    {
        return GetPossibleNewFields(board.GetField(fromIndex), board);
    }

    private List<Field> GetPossibleNewFields(Field fromField, Board board)
    {
        List<Field> possibleFields = new List<Field>();
        foreach (int index in possibleMoveIndices[fromField.FieldIndex])
        {
            Field toField = board.GetField(index);
            if (fromField.CanMoveTo(toField))
            {
                possibleFields.Add(board.GetField(index));
            }
        }
        return possibleFields;
    }

    private List<Field> GetPossibleNewFieldsFlying(Field fromField, Board board)
    {
        List<Field> possibleFields = new List<Field>();
        foreach (var toField in board.Fields)
        {
            if (fromField.CanMoveTo(toField))
            {
                possibleFields.Add(toField);
            }
        }
        return possibleFields;
    }

    static GameState()
    {
        InitializeMills();
        InitializePossibleMoveIndices();
        InitializeFieldNames();
        InitializePlayerNames();
    }

    private static void InitializePlayerNames()
    {
        playerNames = new Dictionary<PlayerNumber, string>();
        playerNames[PlayerNumber.FirstPlayer] = "White";
        playerNames[PlayerNumber.SecondPlayer] = "Black";
    }

    private static void InitializeMills()
    {
        int i = 0;
        POSSIBLE_MILLS = new Mill[N_POSSIBLE_MILLS];
        POSSIBLE_MILLS[i++] = (new Mill(0, 1, 2));
        POSSIBLE_MILLS[i++] = (new Mill(3, 4, 5));
        POSSIBLE_MILLS[i++] = (new Mill(6, 7, 8));
        POSSIBLE_MILLS[i++] = (new Mill(9, 10, 11));
        POSSIBLE_MILLS[i++] = (new Mill(12, 13, 14));
        POSSIBLE_MILLS[i++] = (new Mill(15, 16, 17));
        POSSIBLE_MILLS[i++] = (new Mill(18, 19, 20));
        POSSIBLE_MILLS[i++] = (new Mill(21, 22, 23));
        POSSIBLE_MILLS[i++] = (new Mill(0, 9, 21));
        POSSIBLE_MILLS[i++] = (new Mill(3, 10, 18));
        POSSIBLE_MILLS[i++] = (new Mill(6, 11, 15));
        POSSIBLE_MILLS[i++] = (new Mill(1, 4, 7));
        POSSIBLE_MILLS[i++] = (new Mill(16, 19, 22));
        POSSIBLE_MILLS[i++] = (new Mill(8, 12, 17));
        POSSIBLE_MILLS[i++] = (new Mill(5, 13, 20));
        POSSIBLE_MILLS[i++] = (new Mill(2, 14, 23));
    }

    private static void InitializePossibleMoveIndices()
    {
        possibleMoveIndices = new List<List<int>>(Board.DEFAULT_NUMBER_OF_FIELDS);
        possibleMoveIndices.Add(new List<int> { 1, 9 });
        possibleMoveIndices.Add(new List<int> { 0, 2, 4 });
        possibleMoveIndices.Add(new List<int> { 1, 14 });
        possibleMoveIndices.Add(new List<int> { 4, 10 });
        possibleMoveIndices.Add(new List<int> { 1, 3, 5, 7 });
        possibleMoveIndices.Add(new List<int> { 4, 13 });
        possibleMoveIndices.Add(new List<int> { 7, 11 });
        possibleMoveIndices.Add(new List<int> { 4, 6, 8 });
        possibleMoveIndices.Add(new List<int> { 7, 12 });
        possibleMoveIndices.Add(new List<int> { 0, 10, 21 });
        possibleMoveIndices.Add(new List<int> { 3, 9, 11, 18 });
        possibleMoveIndices.Add(new List<int> { 6, 10, 15 });
        possibleMoveIndices.Add(new List<int> { 8, 13, 17 });
        possibleMoveIndices.Add(new List<int> { 5, 12, 14, 20 });
        possibleMoveIndices.Add(new List<int> { 2, 13, 23 });
        possibleMoveIndices.Add(new List<int> { 11, 16 });
        possibleMoveIndices.Add(new List<int> { 15, 17, 19 });
        possibleMoveIndices.Add(new List<int> { 12, 16 });
        possibleMoveIndices.Add(new List<int> { 10, 19 });
        possibleMoveIndices.Add(new List<int> { 16, 18, 20, 22 });
        possibleMoveIndices.Add(new List<int> { 13, 19 });
        possibleMoveIndices.Add(new List<int> { 9, 22 });
        possibleMoveIndices.Add(new List<int> { 19, 21, 23 });
        possibleMoveIndices.Add(new List<int> { 14, 22 });
    }

    private static void InitializeFieldNames()
    {
        fieldNames = new string[Board.DEFAULT_NUMBER_OF_FIELDS];
        fieldNames[0] = "A1";
        fieldNames[1] = "D1";
        fieldNames[2] = "G1";
        fieldNames[3] = "B2";
        fieldNames[4] = "D2";
        fieldNames[5] = "F2";
        fieldNames[6] = "C3";
        fieldNames[7] = "D3";
        fieldNames[8] = "E3";
        fieldNames[9] = "A4";
        fieldNames[10] = "B4";
        fieldNames[11] = "C4";
        fieldNames[12] = "E4";
        fieldNames[13] = "F4";
        fieldNames[14] = "G4";
        fieldNames[15] = "C5";
        fieldNames[16] = "D5";
        fieldNames[17] = "E5";
        fieldNames[18] = "B6";
        fieldNames[19] = "D6";
        fieldNames[20] = "F6";
        fieldNames[21] = "A7";
        fieldNames[22] = "D7";
        fieldNames[23] = "G7";
    }

    private static HashSet<Mill> GetActiveMills(Board board)
    {
        HashSet<Mill> activeMills = new HashSet<Mill>();
        Mill mill;

        for (int i = 0; i < POSSIBLE_MILLS.Length; i++)
        {
            mill = POSSIBLE_MILLS[i];
            PlayerNumber playerMillNumber = board.GetField(mill.MillIndices[0]).PawnPlayerNumber;
            if (playerMillNumber != PlayerNumber.None)
            {
                bool millPossible = true;
                for (int j = 1; j < mill.MillIndices.Count && millPossible; j++)
                {
                    if (playerMillNumber != board.GetField(mill.MillIndices[j]).PawnPlayerNumber)
                    {
                        millPossible = false;
                    }
                }

                if (millPossible)
                {
                    activeMills.Add(mill);
                }
            }
        }

        return activeMills;
    }
    public MillDifference GetMillDifference(HashSet<Mill> previousMills, Board board)
    {
        HashSet<Mill> activeMills = GetActiveMills(board);
        HashSet<Mill> newActiveMills = new HashSet<Mill>(activeMills);
        newActiveMills.ExceptWith(previousMills);
        return new MillDifference(activeMills, newActiveMills);
    }

    private HashSet<Mill> MillDifference(GameState other)
    {
        HashSet<Mill> millDifference = new HashSet<Mill>(this.ActiveMills);
        millDifference.ExceptWith(other.ActiveMills);
        return millDifference;
    }

    public HashSet<int> GetCurrentPlayerPossibleMoveIndices()
    {
        if(LastSelectedField == null)
        {
            return null;
        }
        List<Field> fields;
        if (CurrentPlayersPawnsLeft <= FLYING_PAWNS_NUMBER)
        {
            fields = GetPossibleNewFieldsFlying(LastSelectedField, CurrentBoard);
        } else
        {
            fields = GetPossibleNewFields(LastSelectedField, CurrentBoard);
        }
        HashSet<int> indices = new HashSet<int>();
        foreach(var field in fields)
        {
            indices.Add(field.FieldIndex);
        }
        return indices;
    }

    private void LogMoveMove(PlayerNumber player, int fieldIndexFrom, int fieldIndexTo)
    {
        LogMove(player, " move " + fieldNames[fieldIndexFrom] + "/" + fieldNames[fieldIndexTo]);
    }

    private void LogPlaceMove(PlayerNumber player, int fieldIndex)
    {
        LogMove(player, " place " + fieldNames[fieldIndex]);
    }

    private void LogRemoveMove(PlayerNumber player, int fieldIndex)
    {
        LogMove(player, " remove " + fieldNames[fieldIndex]);
    }

    private void LogMove(PlayerNumber player, string move)
    {
        MovesUntilNow += playerNames[player] + ": " + move + "\n";
    }
}
