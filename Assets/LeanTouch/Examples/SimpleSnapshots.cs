using UnityEngine;

// This script will set up LineRenderers based on recorded fingers
public class SimpleSnapshots : MonoBehaviour
{
	public LineRenderer[] LineRenderers;
	
	protected virtual void LateUpdate()
	{
		// Does the LineRenderer array exist?
		if (LineRenderers == null)
			return;

		// Go through all LineRenderers
		for (var i = 0; i < LineRenderers.Length; i++)
		{
			// Get the LineRenderer at this index
			var lineRenderer = LineRenderers[i];

			// Has this LineRenderer been set?
			if (lineRenderer == null)
				continue;

			// Find the finger at this index
			var finger = Lean.LeanTouch.Fingers.Find(f => f.Index == i);
					
			// Exists?
			if (finger != null)
			{
				lineRenderer.positionCount = finger.Snapshots.Count;
						
				// Go through all snapshots
				for (var j = 0; j < finger.Snapshots.Count; j++)
				{
					var snapshot = finger.Snapshots[j];
							
					if (snapshot != null)
					{
						lineRenderer.SetPosition(j, snapshot.GetWorldPosition(1.0f));
					}
				}
			}
			// Doesn't exist?
			else
			{
				lineRenderer.positionCount = 0;
			}
		}
	}
}