using UnityEngine;

public class OverlapToolUse : MonoBehaviour, IItemUse
{
    [SerializeField] private int energyCost = 2;

    public int EnergyCost => energyCost;
    public bool ConsumesItem => false;

    public bool TryUse(UseContext context)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(context.ToolOrigin, context.ToolRadius);

        foreach (Collider2D c in colliders)
        {
            ToolHit hit = c.GetComponent<ToolHit>();
            if (hit != null)
            {
                hit.Hit();
                return true; // SUCCESS
            }
        }

        return false; // nothing hit -> no energy spent
    }
}