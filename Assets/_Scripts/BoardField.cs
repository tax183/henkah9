using UnityEngine;

public class BoardField : MonoBehaviour
{
    private Field fieldData; // يستخدم لتخزين بيانات الحقل

    public void SetFieldData(Field field)
    {
        fieldData = field;
    }

    public bool IsEmpty()
    {
        return fieldData == null || fieldData.PawnPlayerNumber == PlayerNumber.None;
    }

    public void PlacePawn(GameObject pawn, PlayerNumber player)
    {
        if (IsEmpty())
        {
            fieldData.PawnPlayerNumber = player;
            pawn.transform.position = transform.position;
        }
    }
}
