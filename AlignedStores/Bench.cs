using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[DisassemblyDiagnoser(exportDiff: true, exportCombinedDisassemblyReport: true)]
public unsafe class Bench
{
    [ModuleInitializer]
    public static void RunCctor()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Bench).TypeHandle);
    }

    private const int Size = 31_250;
    
    private static readonly byte* Source, Dest, DestUnaligned;

    private static readonly int ActualSize;

    static Bench()
    {
        ActualSize = Size * Vector256<byte>.Count;
        
        Source = (byte*) NativeMemory.AlignedAlloc((UIntPtr) ActualSize, (UIntPtr) 64);
        
        Dest = (byte*) NativeMemory.AlignedAlloc((UIntPtr) (ActualSize + Vector256<byte>.Count), (UIntPtr) 64);
        
        DestUnaligned = Dest + 33; //Purposely misalign Dest
    }
    
    [Benchmark]
    public void UnalignedDest()
    {
        var CurrentSource = Source;

        var CurrentDest = DestUnaligned;

        var LastDestOffsetByOne = CurrentDest + ActualSize;

        for (; CurrentDest != LastDestOffsetByOne
             ; CurrentDest += Vector256<byte>.Count
             , CurrentSource += Vector256<byte>.Count)
        {
            Vector256.LoadAligned(CurrentSource).Store(CurrentDest);
        }
    }
    
    [Benchmark]
    public void AlignedDest()
    {
        var CurrentSource = Source;

        var CurrentDest = Dest;

        var LastDestOffsetByOne = CurrentDest + ActualSize;

        for (; CurrentDest != LastDestOffsetByOne
             ; CurrentDest += Vector256<byte>.Count
             , CurrentSource += Vector256<byte>.Count)
        {
            Vector256.LoadAligned(CurrentSource).StoreAligned(CurrentDest);
        }
    }
}