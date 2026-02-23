using UnityEngine;

public class PlayerTeleportService : MonoBehaviour
{
    public void Teleport(Transform player, Transform target)
    {
        if (player == null || target == null) return;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = target.position;
            rb.rotation = target.eulerAngles.z;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            player.position = target.position;
            player.rotation = target.rotation;
        }
    }
}
