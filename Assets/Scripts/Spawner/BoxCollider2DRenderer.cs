using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class BoxCollider2DRenderer : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private LineRenderer lineRenderer;
    public Color color = Color.blue;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 5; // carré fermé
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;

        Vector2 size = boxCollider.size;
        Vector2 offset = boxCollider.offset;

        Vector3[] corners = new Vector3[5]
        {
            new Vector3(offset.x - size.x / 2, offset.y - size.y / 2),
            new Vector3(offset.x - size.x / 2, offset.y + size.y / 2),
            new Vector3(offset.x + size.x / 2, offset.y + size.y / 2),
            new Vector3(offset.x + size.x / 2, offset.y - size.y / 2),
            new Vector3(offset.x - size.x / 2, offset.y - size.y / 2) // fermer le carré
        };

        lineRenderer.SetPositions(corners);
        lineRenderer.widthMultiplier = 0.05f; // ajuste épaisseur selon tes besoins
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = color;
    }
}