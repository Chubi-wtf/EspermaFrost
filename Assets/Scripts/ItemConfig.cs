using UnityEngine;


public class ItemConfig : MonoBehaviour
{
    public ItemTemplate itemTemplate;
    public GameObject objectToActivate;

    private void Start()
    {
        if (itemTemplate != null && GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().material.color = itemTemplate.itemColor;
        }
    }
}