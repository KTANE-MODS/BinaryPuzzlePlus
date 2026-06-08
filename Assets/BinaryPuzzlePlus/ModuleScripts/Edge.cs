public enum EdgeState
{
    None,
    Equal,
    X
}
public class Edge
{
    public EdgeState Constraint;

    public Cell CellA;
    public Cell CellB;
}
