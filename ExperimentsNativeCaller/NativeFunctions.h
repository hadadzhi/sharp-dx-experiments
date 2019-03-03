#pragma once

#include <stdio.h>

#define EXPORT_TO_MANAGED __declspec(dllexport) __stdcall

typedef struct
{

	float x;
	float y;
	float z;

} Vector3;

typedef struct
{

	Vector3 v1;
	float __pad0;
	
	Vector3 v2;
	float __pad1;

} TestStruct;

int EXPORT_TO_MANAGED NativeFunction(void *ptr);
void EXPORT_TO_MANAGED NativeFunctionWithPrimitiveParameters(float arg1, double arg2, int arg3);
