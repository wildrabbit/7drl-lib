public class BaseMapContext
{
    public BaseMapGeneratorData GeneratorData;
    public int noTile;
}

public interface IMapGenerator
{
    void GenerateMap(ref int[] map, BaseMapContext mapGenContext);
}
