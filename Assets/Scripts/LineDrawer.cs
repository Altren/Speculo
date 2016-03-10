using UnityEngine;

class LineDrawer
{
    LineRenderer lineRenderer;
    Vector3 lastPosition = new Vector3(0, 0, 0);
    int pointsCount = 0;

    public LineDrawer(int index, Material lineMaterial)
    {
        lineRenderer = new GameObject("Line" + index).gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.SetWidth(0.15f, 0.15f);
        lineRenderer.useWorldSpace = true;
    }

    public void AddPoint(Component uiItem)
    {
        AddPoint(Camera.main.ScreenToWorldPoint(uiItem.transform.position));
    }

    public void AddPoint(Vector3 position)
    {
        Vector3 prePosition = pointsCount == 0 ? position : Vector3.MoveTowards(position, lastPosition, 0.001f);

        pointsCount++;

        lineRenderer.SetVertexCount(2 * pointsCount);
        lineRenderer.SetPosition(2 * pointsCount - 2, prePosition);
        lineRenderer.SetPosition(2 * pointsCount - 1, position);
        //line.SetPosition(3 * pointsCount - 1, position);
        lastPosition = position;
    }

    public void Reset()
    {
        lineRenderer.SetVertexCount(0);
        pointsCount = 0;
    }
}

