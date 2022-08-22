# AlignedStores

Measuring impact of unaligned stores. Seems to be most impactful at ~8 million bytes. Diminishing returns at ~32 million bytes ( I assume due to cache thrashing )

Note that we are only crossing cache-line boundary 1/2 times. This will be worse when we get AVX512, since it means we cross boundary on every store
