using System;
using System.Numerics;
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
        ActualSize = Size * Vector<byte>.Count;
        
        Source = (byte*) NativeMemory.AlignedAlloc((UIntPtr) ActualSize, (UIntPtr) 64);
        
        Dest = (byte*) NativeMemory.AlignedAlloc((UIntPtr) (ActualSize + Vector<byte>.Count), (UIntPtr) 64);
        
        DestUnaligned = Dest + 33; //Purposely misalign Dest
    }
    
    [Benchmark]
    public void UnalignedDest()
    {
        var CurrentSource = Source;

        var CurrentDest = DestUnaligned;

        var LastDestOffsetByOne = CurrentDest + ActualSize;
        
        if (Vector256.IsHardwareAccelerated)
        {
            for (; CurrentDest != LastDestOffsetByOne
                 ; CurrentDest += Vector256<byte>.Count
                 , CurrentSource += Vector256<byte>.Count)
            {
                Vector256.LoadAligned(CurrentSource).Store(CurrentDest);
            }
        }
        
        else if (Vector128.IsHardwareAccelerated)
        {
            
            for (; CurrentDest != LastDestOffsetByOne
                 ; CurrentDest += Vector128<byte>.Count
                 , CurrentSource += Vector128<byte>.Count)
            {
                Vector128.LoadAligned(CurrentSource).Store(CurrentDest);
            }
        }
        
        else if (Vector64.IsHardwareAccelerated)
        {
            for (; CurrentDest != LastDestOffsetByOne
                 ; CurrentDest += Vector64<byte>.Count
                 , CurrentSource += Vector64<byte>.Count)
            {
                Vector64.LoadAligned(CurrentSource).Store(CurrentDest);
            }
        }

        else
        {
            YourHardwareSucks();
        }
    }
    
    [Benchmark]
    public void AlignedDest()
    {
        var CurrentSource = Source;

        var CurrentDest = Dest;

        var LastDestOffsetByOne = CurrentDest + ActualSize;

        if (Vector256.IsHardwareAccelerated)
        {
            for (; CurrentDest != LastDestOffsetByOne
                 ; CurrentDest += Vector256<byte>.Count
                 , CurrentSource += Vector256<byte>.Count)
            {
                Vector256.LoadAligned(CurrentSource).StoreAligned(CurrentDest);
            }
        }
        
        else if (Vector128.IsHardwareAccelerated)
        {
            
            for (; CurrentDest != LastDestOffsetByOne
                 ; CurrentDest += Vector128<byte>.Count
                 , CurrentSource += Vector128<byte>.Count)
            {
                Vector128.LoadAligned(CurrentSource).StoreAligned(CurrentDest);
            }
        }
        
        else if (Vector64.IsHardwareAccelerated)
        {
            for (; CurrentDest != LastDestOffsetByOne
                 ; CurrentDest += Vector64<byte>.Count
                 , CurrentSource += Vector64<byte>.Count)
            {
                Vector64.LoadAligned(CurrentSource).StoreAligned(CurrentDest);
            }
        }

        else
        {
            YourHardwareSucks();
        }
    }

    private static void YourHardwareSucks()
    {
        throw new Exception("Your hardware sucks!");
    }
}