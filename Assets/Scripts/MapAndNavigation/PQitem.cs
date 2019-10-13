public class PQitem
{
    public float totalDist;
    public GraphNode goThrough;
    public GraphNode node;

    public PQitem(GraphNode origin, GraphNode node)
    {
        goThrough = origin;
        this.node = node;
    }
}