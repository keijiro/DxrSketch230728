using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor.Splines;
#endif

namespace Sketch {

// SplineObserver provides indirect reference to the "after modified" callback
// the Spline editor class. We use this class just for encapsulating the
// dependency on the Editor assembly.
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
