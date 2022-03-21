using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Noise;

public class NoiseVisualization : Visualization {
    private static readonly int noiseId = Shader.PropertyToID("_Noise");

    static ScheduleDelegate[] noiseJobs = {
        Job<Lattice1D>.ScheduleParallel,
        Job<Lattice2D>.ScheduleParallel,
        Job<Lattice3D>.ScheduleParallel
    };

    [SerializeField] private int seed;
    [SerializeField] SpaceTRS domain = new SpaceTRS() {scale = 8f};
    [SerializeField, Range(1, 3)] private int dimensions = 3;

    private NativeArray<float4> noise;

    private ComputeBuffer noiseBuffer;

    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {
        noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
        noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
        propertyBlock.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void DisableVisualization() {
        noise.Dispose();
        noiseBuffer.Release();
        noiseBuffer = null;
    }

    protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle) {
        noiseJobs[dimensions - 1](
            positions, noise, seed, domain, resolution, handle
        ).Complete();
        noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
    }
}