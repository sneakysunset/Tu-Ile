using System;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class UniformTransformRelaxation
    {
        /// <summary>
        /// Iteratively relaxes voronoi sites towards equal area regions.
        /// This archieved by optimizing function: F(x) = f_0(x)^2 + f_1(x)^2 ... f_n(x)^2, where f_i(x) = region_area_i - region_goal_area.
        /// Optimization is done using Levenberg Marquardt algorithm: x_new = x + (-inv(H * lamba*I)*g(x)).
        /// Based on https://docs.mrpt.org/reference/latest/page_tutorial_math_levenberg_marquardt.html.
        /// </summary>
        public static float Execute(ref VoronoiBuilder voronoiBuilder, ref VoronoiDiagram voronoiDiagram, ref NativeArray<float2> sites, ref float4 x0, int numIterations, float stepSize)
        {
            if (stepSize <= 0)
                throw new ArgumentException("Step size must be greater than zero.");
            if (numIterations == 0)
                throw new ArgumentException("Number of interations must be greater than zero.");

            float4 x = x0;
            float F_x = ErrorFunction(ref voronoiBuilder, ref voronoiDiagram, sites, x);

            float4 f_x = Function(ref voronoiBuilder, ref voronoiDiagram, sites, x);
            float4x4 J = JacobianMatrix(ref voronoiBuilder, ref voronoiDiagram, sites, x);
            float4 g = Gradient(f_x, J);

            // Hessian is approximated into this
            float4x4 H = HessianMatrix(J);

            float lambda = stepSize * math.max(H.c0.x, H.c1.y);
            float v = 2;

            // small lambda -> Gauss–Newton algorithm -> moves fast
            // big lambda -> gradient-descent -> moves slow

            for (int i = 0; i < numIterations; ++i)
            {
                float4x4 lambdaI = lambda * float4x4.identity;
                float4x4 invH = math.inverse(H + lambdaI);

                Assert.IsFalse(math.any(math.isnan(invH.c0)));
                Assert.IsFalse(math.any(math.isnan(invH.c1)));

                // H_lm = -( H + \lambda I ) ^-1 * g
                float4 h_lm = -math.mul(invH, g);

                if (math.all(math.abs(h_lm) < 1e-6f))
                    break;

                float4 xnew = x + h_lm;

                float F_xnew = ErrorFunction(ref voronoiBuilder, ref voronoiDiagram, sites, xnew);

                // denom = h_lm^t * ( \lambda * h_lm - g )
                var tmp = h_lm;
                tmp *= lambda;
                tmp -= g;
                float denom = math.dot(tmp, h_lm);
                float l = (F_x - F_xnew) / denom;

                if (l > 0) // There is an improvement:
                {
                    //UnityEngine.Debug.Log($"Iteration:{i} improvement:{F_x}->{F_xnew} lambda:{lambda}");

                    // Accept new point:
                    x = xnew;
                    F_x = F_xnew;

                    // As x changed we need to recalculate
                    f_x = Function(ref voronoiBuilder, ref voronoiDiagram, sites, x);
                    J = JacobianMatrix(ref voronoiBuilder, ref voronoiDiagram, sites, x);
                    g = Gradient(f_x, J);
                    H = HessianMatrix(J);

                    lambda *= math.max(0.33f, 1 - math.pow(2 * l - 1, 3));
                    v = 2;
                }
                else
                {
                    // Nope...
                    lambda *= v;
                    v *= 2;

                    //UnityEngine.Debug.Log($"Iteration:{i} rejected:{F_x}->{F_xnew} lambda:{lambda}");
                }
            }

            for (int siteIndex = 0; siteIndex < sites.Length; ++siteIndex)
            {
                float scale = x.z > 0 ? 1 + x.z : 1f / (1 - x.z);
                sites[siteIndex] = (sites[siteIndex] * scale) + x.xy;
            }

            x0 = x;

            return F_x;
        }

        static float4 Gradient(float4 f, float4x4 jacobianMatrix)
        {
            float4 g = math.mul(math.transpose(jacobianMatrix), f);
            return g;
        }

        /// <summary>
        /// This is the function we are trying to optimize to zero: f_i(x) = abs(region_area_i - region_goal_area).
        /// </summary>
        static float4 Function(ref VoronoiBuilder voronoiBuilder, ref VoronoiDiagram voronoiDiagram, NativeArray<float2> sites, float4 x)
        {
            float goalArea = (voronoiDiagram.Bounds.width * voronoiDiagram.Bounds.height) / sites.Length;
            float4 values;
            if (voronoiDiagram.Regions.Length == 4)
            {
                float scale = 1;//x.z > 0 ? 1 + x.z : 1f / (1 - x.z);
                voronoiBuilder.SetSites((sites[0] * scale) + x.xy, (sites[1] * scale) + x.xy, (sites[2] * scale) + x.xy, (sites[3] * scale) + x.xy);
                voronoiBuilder.Construct(ref voronoiDiagram, voronoiDiagram.Bounds);
                values = new float4(
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[0]),
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[1]),
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[2]),
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[3]));
            }
            else if (voronoiDiagram.Regions.Length == 3)
            {
                float scale = x.z > 0 ? 1 + x.z : 1f / (1 - x.z);
                voronoiBuilder.SetSites((sites[0] * scale) + x.xy, (sites[1] * scale) + x.xy, (sites[2] * scale) + x.xy);
                voronoiBuilder.Construct(ref voronoiDiagram, voronoiDiagram.Bounds);
                values = new float4(
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[0]),
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[1]),
                    voronoiDiagram.GetRegionArea(voronoiDiagram.Regions[2]),
                    goalArea); // skips 4 component
            }
            else
            {
                throw new NotImplementedException();
            }
            return math.abs(values - goalArea); // TODO: find out if this abs is needed
        }
        
        static float ErrorFunction(ref VoronoiBuilder voronoiBuilder, ref VoronoiDiagram voronoiDiagram, NativeArray<float2> sites, float4 x)
        {
            float4 fx = Function(ref voronoiBuilder, ref voronoiDiagram, sites, x);
            return math.dot(fx, fx);
        }

        /// <summary>
        /// First order derivatives of Function.
        /// Based on https://en.wikipedia.org/wiki/Jacobian_matrix_and_determinant
        /// </summary>
        static float4x4 JacobianMatrix(ref VoronoiBuilder voronoiBuilder, ref VoronoiDiagram voronoiDiagram, NativeArray<float2> sites, float4 x)
        {
            float delta = 1e-3f;
            float4 f_x = Function(ref voronoiBuilder, ref voronoiDiagram, sites, x);
            float4 dx = (Function(ref voronoiBuilder, ref voronoiDiagram, sites, x + new float4(delta, 0, 0, 0)) - f_x) / delta;
            float4 dy = (Function(ref voronoiBuilder, ref voronoiDiagram, sites, x + new float4(0, delta, 0, 0)) - f_x) / delta;
            float4 dz = (Function(ref voronoiBuilder, ref voronoiDiagram, sites, x + new float4(0, 0, delta, 0)) - f_x) / delta;
            float4 dw = new float4(0, 0, 0, 1);// (Function(ref voronoiBuilder, ref voronoiDiagram, sites, x + new float4(0, 0, 0, delta)) - f_x) / delta;
            //UnityEngine.Debug.Log($"{dx} {dy}");
            return new float4x4(dx, dy, dz, dw);
        }

        /// <summary>
        /// Second order derivatives of Function.
        /// </summary>
        static float4x4 HessianMatrix(float4x4 jacobianMatrix)
        {
            // Hessian is approximated into this
            float4x4 H = math.mul(math.transpose(jacobianMatrix), jacobianMatrix);
            return H;
        }
    }
}
