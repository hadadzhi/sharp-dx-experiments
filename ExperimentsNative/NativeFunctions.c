#include "NativeFunctions.h"

void PrintVector3(Vector3 v)
{
	printf("(%f, %f, %f)\n", v.x, v.y, v.z);
}

int EXPORT_TO_MANAGED NativeFunction(void *ptr)
{
	TestStruct *tptr = (TestStruct *) ptr;

//	PrintVector3(tptr->v1);
//	PrintVector3(tptr->v2);

	float x1 = tptr->v1.x;
	float y2 = tptr->v2.y;

	return (x1 == 0 && y2 == 1) ? 1 : 0;
}

void EXPORT_TO_MANAGED NativeFunctionWithPrimitiveParameters(float arg1, double arg2, int arg3)
{
}
