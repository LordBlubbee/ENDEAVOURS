using UnityEngine;

public class DrifterCollider : MonoBehaviour
{
    public Collider2D Collider;
    public DRIFTER Drifter;

    // Set this to the layer your Drifter GameObjects use
    public LayerMask DrifterLayer;

    private Collider2D[] results = new Collider2D[8];

    private void FixedUpdate()
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
            Debug.Log(col.name);
            if (col == Collider) continue; // Skip self

            Vector3 away = (Drifter.transform.position - col.transform.position).normalized;

            Drifter.transform.position += away * -Collider.Distance(col).distance * CO.co.GetWorldSpeedDeltaFixed();
        }
    }
}   
