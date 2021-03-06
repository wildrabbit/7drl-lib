public class BlockingEntity : BaseEntity, IBlockingEntity
{
    public BlockingTrait BlockingTrait => _blockingTrait;
    BlockingTrait _blockingTrait;

    public override void AddTime(float timeUnits, ref int playContext)
    {}

    public void Unlock(BaseEntity unlockEntity)
    {
        _entityController.DestroyEntity(this);
    }

    protected override void DoInit(BaseEntityDependencies dependencies)
    {
        var blockingData = (BlockingData)_entityData;
        _blockingTrait = new BlockingTrait();
        _blockingTrait.Init(this, blockingData.BlockingTrait, dependencies.GameEvents.Blocks);
    }
}
