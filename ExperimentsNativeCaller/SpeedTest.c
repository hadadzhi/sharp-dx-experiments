#include <Windows.h>
#include "NativeFunctions.h"

#define MAX_CALLS (100000)

int main()
{
	LARGE_INTEGER freq;
	LARGE_INTEGER ticks1;
	LARGE_INTEGER ticks2;
	
	int i;
	int calls = 0;
	int success = 0;
	double delta;

	// 1
	Vector3 v1 = {0, 0, 0};
	Vector3 v2 = {1, 1, 1};
	TestStruct t;

	L"zzz";

	t.v1 = v1;
	t.v2 = v2;

	// 2
	if (!QueryPerformanceFrequency(&freq))
	{
		printf("Error querying perf timer frequency\n");
		return 1;
	}

	// 3
	QueryPerformanceCounter(&ticks1);

	for (i = 0; i < MAX_CALLS; i++)
	{
		success = NativeFunction(&t);
		calls++;

		if (!success)
		{
			break;
		}
	}

	QueryPerformanceCounter(&ticks2);
	delta = (double) (ticks2.QuadPart - ticks1.QuadPart) / freq.QuadPart * 1000;

	printf("%d calls through native function from native code: %.1lf ms\n", calls, delta);

	// 4
	calls = 0;
	QueryPerformanceCounter(&ticks1);

	for (i = 0; i < MAX_CALLS; i++)
	{
		NativeFunctionWithPrimitiveParameters(1, 1, 1);
		calls++;
	}

	QueryPerformanceCounter(&ticks2);
	delta = (double) (ticks2.QuadPart - ticks1.QuadPart) / freq.QuadPart * 1000;

	printf("%d calls through another native function from native code: %.1lf ms\n", calls, delta);

	getchar();
}
