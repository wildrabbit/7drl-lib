public class BaseMapContext
{
    public BaseMapGeneratorData GeneratorData;
}

public interface IMapGenerator
{
    void GenerateMap(ref int[] map, BaseMapContext mapGenContext);
}
