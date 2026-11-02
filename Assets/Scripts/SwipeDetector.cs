using UnityEngine;

public class SwipeDetector {

    private Vector2 startPosition;
    private Vector2 anchorPosition;
    private Vector2 lastPosition;
    private bool isTracking;
    private bool hasDetectedDuringTouch;

    public void Begin(Vector2 touchStartPosition) {
        startPosition = touchStartPosition;
        anchorPosition = touchStartPosition;
        lastPosition = touchStartPosition;
        
        isTracking = true;
        hasDetectedDuringTouch = false;
    }
    
    public bool End(Vector2 releasePosition, float thresholdPixels, out Vector2 direction) {
        direction = Vector2.zero;

        if (!isTracking) return false;
        isTracking = false;

        if (hasDetectedDuringTouch || thresholdPixels <= 0f) return false;

        Vector2 endPosition = releasePosition == Vector2.zero ? lastPosition : releasePosition;
        Vector2 delta = endPosition - startPosition;

        if (delta.sqrMagnitude < thresholdPixels * thresholdPixels) return false;

        direction = ToCardinalDirection(delta);
        return true;
    }

    public bool TryToDetectSwipe(Vector2 currentPosition, float thresholdPixels, out Vector2 direction) {
        direction = Vector2.zero;

        if (!isTracking || thresholdPixels <= 0f) return false;

        lastPosition = currentPosition;

        Vector2 delta = currentPosition - anchorPosition;

        if (delta.sqrMagnitude < thresholdPixels * thresholdPixels) return false;

        direction = ToCardinalDirection(delta);

        anchorPosition = currentPosition;
        hasDetectedDuringTouch = true;
        return true;
    }
    
    private static Vector2 ToCardinalDirection(Vector2 delta) {
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? new Vector2(Mathf.Sign(delta.x), 0f)
            : new Vector2(0f, Mathf.Sign(delta.y));
    }
}
