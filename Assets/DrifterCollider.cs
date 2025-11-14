using UnityEngine;

public class DrifterCollider : MonoBehaviour
{
    public Collider2D Collider;

    // Set this to the layer your Drifter GameObjects use
    public LayerMask DrifterLayer;

    private Collider2D[] results = new Collider2D[32];

    private void Update()
    {
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = DrifterLayer,
            useTriggers = true
        };

        int count = Collider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            if (col == Collider) continue; // Skip self

            Vector3 away = (transform.position - col.transform.position).normalized;
            Debug.Log($"Pushing away: {away * -Collider.Distance(col).distance} per second");
            transform.position += away * -Collider.Distance(col).distance * CO.co.GetWorldSpeedDelta();
        }
    }
}   
