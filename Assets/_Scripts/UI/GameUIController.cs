﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{

    private static readonly int HUMAN_DROPDOWN_NUMBER = 0;
    private static readonly int AI_DROPDOWN_NUMBER = 1;
    private static readonly int MIN_MAX_DROPDOWN_NUMBER = 0;
    private static readonly int ALPHA_BETA_DROPDOWN_NUMBER = 1;
    private static readonly int FAST_ALPHA_BETA_DROPDOWN_NUMBER = 1;

    private static Dictionary<int, Func<Heuristic>> heuristicDictionary;
    [SerializeField] private GameObject[] yellowPawns; // الأحجار الصفراء
    [SerializeField] private GameObject[] redPawns; // الأحجار الحمراء

    [SerializeField] private TMP_Dropdown firstPlayerTypeDropdown = null;
    [SerializeField] private TMP_Dropdown firstPlayerAlgorithmDropdown = null;
    [SerializeField] private TMP_Dropdown firstPlayerHeuristicDropdown = null;
    [SerializeField] private TMP_Dropdown firstPlayerSearchDepthDropdown = null;

    [SerializeField] private TMP_Dropdown secondPlayerTypeDropdown = null;
    [SerializeField] private TMP_Dropdown secondPlayerAlgorithmDropdown = null;
    [SerializeField] private TMP_Dropdown secondPlayerHeuristicDropdown = null;
    [SerializeField] private TMP_Dropdown secondPlayerSearchDepthDropdown = null;

    [SerializeField] private TextMeshProUGUI numberOfMovesText = null;
    [SerializeField] private TextMeshProUGUI timerText = null;

    [SerializeField] private string numberOfMovesTemplateText = "Moves: {0}";
    [SerializeField] private string timerTemplateText = "Time[s]: {0}";
    [SerializeField] private string currentMovingPlayerTemplateText = "Turn: Player {0}";
    [SerializeField] private TextMeshProUGUI currentMovingPlayerText = null;

    [SerializeField] private Button playButton = null;
    [SerializeField] private Toggle logToFileToggle = null;

    [SerializeField] private Button[] pawnButtons = null;

    [SerializeField] private Sprite firstPlayerPawnImage = null;
    [SerializeField] private Sprite secondPlayerPawnImage = null;
    [SerializeField] private Sprite emptyField = null;
    [SerializeField] private string winningPlayerTextTemplate = "Won: ";
    [SerializeField] private TextMeshProUGUI winningPlayerTextField = null;
    // المتغيرات الخاصة بالـ Popups
    [SerializeField] private GameObject gameModePopup; // يحدد محلي أو المحنكة
    [SerializeField] private GameObject difficultyPopup; // يحدد مستوى الصعوبة
    [SerializeField] private GameObject startPopup; // يحتوي على زر "ابدأ اللعب"
    [SerializeField] private GameObject board;

    [SerializeField] private Button localButton;
    [SerializeField] private Button m7nkaButton;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button startGameButton;

    private Color emptyColor = new Color(255, 255, 255, 0);
    private Color nonEmptyColor = new Color(255, 255, 255, 255);

    private Color firstPlayerColor = new Color(255, 255, 255, 255);
    private Color secondPlayerColor = new Color(0, 0, 0, 255);




    private float timePassed;
    private bool shouldLogToFile;
    private GameEngine gameEngine = null;
    private PlayersController aiPlayersController = null;


    private bool isAI = false;
    private int searchDepth = 1; // افتراضيًا يكون سهل
    private GameObject selectedPawn = null; // الحجر الذي يتم سحبه
    private Vector3 lastValidPosition; // آخر موقع صالح للحجر
    private CanvasGroup canvasGroup;
    [SerializeField] private Transform[] allowedPositions; // جميع أماكن الأحجار على البورد
    private int player1Stones = 9;
    private int player2Stones = 9;

    private static Dictionary<Vector3, GameObject> occupiedPositions = new Dictionary<Vector3, GameObject>(); // تخزين الأحجار في البورد


static GameUIController()
    {
        heuristicDictionary = new Dictionary<int, Func<Heuristic>>();
        heuristicDictionary[0] = () => new SimplePawnNumberHeuristic();
        heuristicDictionary[1] = () => new PawnMillNumberHeuristic();
        heuristicDictionary[2] = () => new PawnMoveNumberHeuristic();
    }
    private void Awake()
    {
        board.SetActive(false); // تعطيل البورد عند بدء اللعبة

        InitPawnButtonHandlers();

        localButton.onClick.AddListener(SelectLocal);
        m7nkaButton.onClick.AddListener(SelectM7nka);
        easyButton.onClick.AddListener(() => SelectDifficulty(1));
        mediumButton.onClick.AddListener(() => SelectDifficulty(2));
        hardButton.onClick.AddListener(() => SelectDifficulty(3));
        startGameButton.onClick.AddListener(StartGame);

        ShowGameModePopup();
    }

    private void InitPawnButtonHandlers()
    {
        for (int i = 0; i < pawnButtons.Length; i++)
        {
            int x = i;
            pawnButtons[i].onClick.AddListener(() => HandleButtonClick(x));
        }
    }
    public void UpdateStonesUI(int player)
    {
        if (GameEngine.Instance == null) return; // تأكد أن GameEngine موجود

        int stonesLeft = (player == 1) ? GameEngine.Instance.GameState.FirstPlayersPawnsToPlaceLeft
                                       : GameEngine.Instance.GameState.SecondPlayersPawnsToPlaceLeft;

        Debug.Log($"🎯 تحديث للأحجار! لاعب {player} لديه {stonesLeft} حجر متبقي.");

        if (stonesLeft >= 0 && stonesLeft < yellowPawns.Length)
        {
            if (player == 1)
            {
                yellowPawns[stonesLeft].SetActive(false);
            }
            else
            {
                redPawns[stonesLeft].SetActive(false);
            }
        }
    }


    private void ShowGameModePopup()
    {
        gameModePopup.SetActive(true);
        difficultyPopup.SetActive(false);
    }

    public void SelectLocal()
    {
        isAI = false;
        gameModePopup.SetActive(false);
        StartGame(); // ✅ يبدأ اللعب مباشرة بدون نافذة اختيار المستوى
    }

    public void SelectM7nka()
    {
        isAI = true;
        gameModePopup.SetActive(false);
        ShowDifficultyPopup();
        // ✅ عرض نافذة اختيار المستوى فقط إذا كان الذكاء الاصطناعي مفعلاً
    }


    private void ShowDifficultyPopup()
    {
        difficultyPopup.SetActive(true);
    }

    public void SelectDifficulty(int depth)
    {
        searchDepth = depth;
        difficultyPopup.SetActive(false);
        StartGame(); // بعد اختيار المستوى، يبدأ اللعب مباشرة
    }

    // ✅ زر "كانسل" في `Game Mode Popup`
    public void CancelGameModePopup()
    {
        gameModePopup.SetActive(false);

        SceneManager.LoadScene("um9 menu page");
    }

    // ✅ زر "كانسل" في `Difficulty Popup` يعيد اللاعب إلى `Game Mode Popup`
    public void CancelDifficultyPopup()
    {
        difficultyPopup.SetActive(false);
        ShowGameModePopup();
    }


    void StartGame()
    {
        board.SetActive(true); // تفعيل البورد عند بدء اللعب

        gameEngine = new GameEngine();
        AiPlayer firstPlayer = null;

        AiPlayer secondPlayer = isAI
            ? new FastAlphaBetaAiPlayer(
                gameEngine,
                new CombinedHeuristic(searchDepth), // الهيرستك الذكي
                PlayerNumber.SecondPlayer,
                searchDepth,
                new SimplePawnNumberHeuristic()     // فرز سريع
            )
            : null;

        aiPlayersController = new PlayersController(firstPlayer, secondPlayer);
        timePassed = 0;
        OnBoardUpdated(gameEngine.GameState.CurrentBoard);
        gameEngine.OnBoardChanged += OnBoardUpdated;
        gameEngine.OnGameFinished += OnGameFinished;
        gameEngine.OnPlayerTurnChanged += OnPlayerTurnChanged;
        gameEngine.OnPlayerTurnChanged += aiPlayersController.OnPlayerTurnChanged;
        gameEngine.OnLastFieldSelectedChanged += UpdatePossibleMoveIndicators;
        UpdateWinningPlayerText(PlayerNumber.None);
        playButton.interactable = false;

        // ✅ 🔹 تجهيز الأحجار لكل لاعب
        for (int i = 0; i < 9; i++)
        {
            yellowPawns[i].SetActive(true);
            redPawns[i].SetActive(true);
        }

        // ✅ 🔹 ضبط أماكن الأحجار
        GameObject allowedPositionsObject = GameObject.Find("AllowedPositions");
        if (allowedPositionsObject != null)
        {
            allowedPositions = allowedPositionsObject
                .GetComponentsInChildren<Transform>()
                .Where(t => t != allowedPositionsObject.transform)
                .ToArray();
        }

        Debug.Log("🎯 تم تجهيز الأحجار وإعداد البورد!");
        Debug.Log($"✅ بدأ اللعب: Player1 = Human | Player2 = {(isAI ? "AI" : "Human")} | Depth = {searchDepth}");
    }

    private AiPlayer InitPlayer(PlayerNumber playerNumber)
    {
        if (playerNumber == PlayerNumber.FirstPlayer)
        {
            return null;
        }

        if (isAI)
        {
            Heuristic bestHeuristic = new CombinedHeuristic(searchDepth); // دمج حسب الصعوبة
            Heuristic sortHeuristic = new SimplePawnNumberHeuristic();    // فرز خفيف

            return new FastAlphaBetaAiPlayer(gameEngine, bestHeuristic, playerNumber, searchDepth, sortHeuristic);
        }

        return null;
    }


    private void OnBoardUpdated(Board newBoard)
    {
        for (int i = 0; i < pawnButtons.Length; i++)
        {
            Field field = newBoard.GetField(i);
            if (field.PawnPlayerNumber == PlayerNumber.FirstPlayer)
            {
                pawnButtons[i].image.sprite = firstPlayerPawnImage;
                pawnButtons[i].image.color = nonEmptyColor;
            }
            else if (field.PawnPlayerNumber == PlayerNumber.SecondPlayer)
            {
                pawnButtons[i].image.sprite = secondPlayerPawnImage;
                pawnButtons[i].image.color = nonEmptyColor;
            }
            else
            {
                pawnButtons[i].image.color = emptyColor;
            }
        }
    }

    private void OnPlayerTurnChanged(PlayerNumber currentMovingPlayerNumber)
    {
        if (currentMovingPlayerNumber == PlayerNumber.FirstPlayer)
        {
            UpdateTurnText(1);
        }
        else
        {
            UpdateTurnText(2);
        }
    }

    private void UpdateTurnText(int playerNumber)
    {
        currentMovingPlayerText.text = string.Format(currentMovingPlayerTemplateText, playerNumber);
        currentMovingPlayerText.faceColor = playerNumber == 1 ? firstPlayerColor : secondPlayerColor;
    }

    private void OnGameFinished(PlayerNumber winningPlayer)
    {
        UpdateWinningPlayerText(winningPlayer);
        SaveLogs();
        gameEngine.OnBoardChanged -= OnBoardUpdated;
        gameEngine.OnGameFinished -= OnGameFinished;
        gameEngine.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        gameEngine.OnPlayerTurnChanged -= aiPlayersController.OnPlayerTurnChanged;
        gameEngine.OnLastFieldSelectedChanged -= UpdatePossibleMoveIndicators;
        gameEngine = null;
        aiPlayersController = null;
        playButton.interactable = true;
    }

    private void SaveLogs()
    {
        if (shouldLogToFile)
        {
            string moves = gameEngine.GameState.MovesUntilNow;
            try
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt", moves);
            }
            catch (Exception e)
            {

            }
        }
    }

    private void UpdateWinningPlayerText(PlayerNumber winningPlayer)
    {
        Color winningPlayerColor = firstPlayerColor;
        string winningPlayerString = "Player 1";
        if (winningPlayer == PlayerNumber.SecondPlayer)
        {
            winningPlayerString = "Player 2";
            winningPlayerColor = secondPlayerColor;
        }
        else if (winningPlayer == PlayerNumber.None)
        {
            winningPlayerString = "";
        }
        winningPlayerTextField.text = winningPlayerTextTemplate + winningPlayerString;
        winningPlayerTextField.faceColor = winningPlayerColor;
    }

    private void HandleButtonClick(int fieldIndex)
    {
        if (gameEngine != null)
        {
            gameEngine.HandleSelection(fieldIndex);
        }
    }

    private void Update()
    {
        if (isAI) // فقط نفذ خطوة الذكاء الاصطناعي إذا كان هناك AI
        {
            MakeAiControllerStep();
        }
        UpdateGameStateData();
    }


    private void MakeAiControllerStep()
    {
        if (aiPlayersController != null)
        {
            long timeMilis = aiPlayersController.CheckStep();
            timePassed += timeMilis / 1000f;
        }
    }

    private void UpdateGameStateData()
    {
        if (gameEngine != null)
        {
            UpdateMoveNumberText();
            UpdateTime();
        }
    }

    private void UpdateTime()
    {
        if (!gameEngine.GameState.GameFinished)
        {
            timePassed += Time.deltaTime;
            timerText.text = string.Format(timerTemplateText, Math.Truncate(timePassed * 100) / 100);
        }
    }

    private void UpdateMoveNumberText()
    {
        numberOfMovesText.text = string.Format(numberOfMovesTemplateText, gameEngine.GameState.MovesMade);
    }

    private void UpdatePossibleMoveIndicators()
    {
        HashSet<int> possibleMoveIndices = gameEngine.GetCurrentPossibleMoves();
        if (possibleMoveIndices == null)
        {
            for (int i = 0; i < pawnButtons.Length; i++)
            {
                Image[] images = pawnButtons[i].GetComponentsInChildren<Image>();
                images[1].enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < pawnButtons.Length; i++)
            {
                Image[] images = pawnButtons[i].GetComponentsInChildren<Image>();
                images[1].enabled = possibleMoveIndices.Contains(i);
            }
        }
    }
    public class CombinedHeuristic : Heuristic
    {
        private SimplePawnNumberHeuristic simple = new SimplePawnNumberHeuristic();
        private PawnMillNumberHeuristic mill = new PawnMillNumberHeuristic();
        private PawnMoveNumberHeuristic move = new PawnMoveNumberHeuristic();

        private double simpleWeight;
        private double millWeight;
        private double moveWeight;

        public CombinedHeuristic(int difficulty)
        {
            switch (difficulty)
            {
                case 1: // سهل
                    simpleWeight = 1.5;
                    millWeight = 0.5;
                    moveWeight = 0.3;
                    break;
                case 2: // متوسط
                    simpleWeight = 1.0;
                    millWeight = 1.0;
                    moveWeight = 1.0;
                    break;
                case 3: // صعب
                    simpleWeight = 0.8;
                    millWeight = 1.5;
                    moveWeight = 1.2;
                    break;
            }
        }

        public double Evaluate(GameState gameState)
        {
            double eval1 = simple.Evaluate(gameState);
            double eval2 = mill.Evaluate(gameState);
            double eval3 = move.Evaluate(gameState);
            return (eval1 * simpleWeight) + (eval2 * millWeight) + (eval3 * moveWeight);
        }
    }

}