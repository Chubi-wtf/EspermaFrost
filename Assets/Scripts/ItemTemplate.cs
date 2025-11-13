using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class ItemTemplate : ScriptableObject
{
    public string itemName;
    public Color itemColor;
    public enum ITEM_TYPE
    {
        None,
        KeyCard,
        Botiquin,
        Adrenalina
    }
    public ITEM_TYPE itemType;

    [Header("Datos de Uso General")]
    public float useDuration;

    [Header("Datos de Botiquín")]
    public float healAmount;

    [Header("Datos de Adrenalina")]
    public float adrenalineDuration;
}