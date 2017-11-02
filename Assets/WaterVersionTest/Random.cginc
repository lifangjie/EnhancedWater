#ifndef RANDOM_CG_INCLUDED
#define RANDOM_CG_INCLUDED

//todo: some problems need to solve
//example :
// SetSeed(id.x * 512 + id.y);
// x1 = 2 * Rand() - 1;
// x2 = 2 * Rand() - 1;

uint rng_state;

uint rand_lcg()
{
    // LCG values from Numerical Recipes
    rng_state = 1664525 * rng_state + 1013904223;
    return rng_state;
}

uint wang_hash(uint seed)
{
    seed = (seed ^ 61) ^ (seed >> 16);
    seed *= 9;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2d;
    seed = seed ^ (seed >> 15);
    return seed;
}
void SetSeed(uint seed)
{
    rng_state = wang_hash(seed);
}
float Rand()
{
    return float(rand_lcg())/4294967296.0;
}
#endif