using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;

public struct AsteroidGhostSerializer : IGhostSerializer<AsteroidSnapshotData>
{
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    private ComponentType componentTypeAsteroidTagComponentData;
    private ComponentType componentTypeRotation;
    [NativeDisableContainerSafetyRestriction] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    private ComponentType componentTypeTranslation;
    [NativeDisableContainerSafetyRestriction] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public bool WantsPredictionDelta => true;

    public int SnapshotSize => UnsafeUtility.SizeOf<AsteroidSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeAsteroidTagComponentData = ComponentType.ReadWrite<AsteroidTagComponentData>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>();

    }

    public bool CanSerialize(EntityArchetype arch)
    {
        var components = arch.GetComponentTypes();
        int matches = 0;
        for (int i = 0; i < components.Length; ++i)
        {
            if (components[i] == componentTypeAsteroidTagComponentData)
                ++matches;
            if (components[i] == componentTypeRotation)
                ++matches;
            if (components[i] == componentTypeTranslation)
                ++matches;

        }
        return (matches == 3);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref AsteroidSnapshotData snapshot)
    {
        snapshot.tick = tick;
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value);

    }
}
