using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Extrapolation : MonoBehaviour {

    public static float InterpolateX(float[] t, float[] y, float x) {
        float answer = 0;

        List<float> allX = new List<float>();
        List<float> allY = new List<float>();

        for (int i = 0; i < t.Length; i++) {
            allX.Add(t[i]);
        }

        for(int i = 0;i < y.Length;i++) {
            allY.Add(y[i]);
        }

        for(int i = 0;i <= allX.Count - 1;i++) {
            float numerator = 1;
            float denominator = 1;
            for(int c = 0;c <= allX.Count - 1;c++) {
                if(c != i) {
                    numerator *= (x - allX[c]);
                    denominator *= (allX[i] - allX[c]);

                    if (denominator == 0) {
                        Debug.LogError("BOOOOM" + allX[i] + " " + allX[c]);
                    }
                }
            }
            answer += allY[i] * (numerator / denominator);
        }
        return answer;
    }

    public static float GetValue(Vector2[] controlPoints, float x) {
        float result = 0f;
        for(int j = 0;j < controlPoints.Length;j++) {
            result += Inner(controlPoints, x, j);
        }
        return result;
    }

    private static float Inner(Vector2[] controlPoints, float x, int j) {
        float dividend = 1f;
        float divisor = 1f;
        for(int k = 0;k < controlPoints.Length;k++) {
            if(k == j) {
                continue;
            }
            dividend *= (x - controlPoints[k].x);
            divisor *= (controlPoints[j].x - controlPoints[k].x);
        }
        return (dividend / divisor) * controlPoints[j].y;
    }
}
