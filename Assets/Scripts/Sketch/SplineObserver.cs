using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor.Splines;
#endif

namespace Sketch {

public static class SplineObserver
{
    public static event System.Action<Spline> OnModified;

#if UNITY_EDITOR
    static SplineObserver()
      => EditorSplineUtility.AfterSplineWasModified +=
           (Spline spline) => OnModified(spline);
#endif
}

} // namespace Sketch
