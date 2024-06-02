using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace NeuroRehab.Core {
	public class LerpUtility {
		/// <summary>
		/// Coroutine for Linear interpolation for weights, we slowly move arm towards our goal. Refer to https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/ for more details
		/// </summary>
		/// <param name="rig"></param>
		/// <param name="lerpDuration"></param>
		/// <param name="startLerpValue"></param>
		/// <param name="endLerpValue"></param>
		/// <returns></returns>
		public IEnumerator SimpleRigLerp(Rig rig, float lerpDuration, float startLerpValue, float endLerpValue) {
			float lerpTimeElapsed = 0f;

			while (lerpTimeElapsed < lerpDuration) {
				// despite the fact lerp should be linear, we use calculation to manipulate it, the movement then looks more natural
				float t = lerpTimeElapsed / lerpDuration;
				t = t * t * t * (t * (6f * t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
				rig.weight = Mathf.Lerp(startLerpValue, endLerpValue, t);
				lerpTimeElapsed += Time.deltaTime;
				yield return null;
			}
			// lerp never reaches endValue, that is why we have to set it manually
			rig.weight = endLerpValue;
		}

		/// <summary>
		/// Coroutine that is the same as simpleRigLerp but for as many rigs as needed
		/// </summary>
		/// <param name="rigLerps"></param>
		/// <param name="lerpDuration"></param>
		/// <returns></returns>
		public IEnumerator MultiRigLerp(RigLerp[] rigLerps, float lerpDuration) {
			float lerpTimeElapsed = 0f;

			while (lerpTimeElapsed < lerpDuration) {
				// despite the fact lerp should be linear, we use calculation to manipulate it, the movement then looks more natural
				float t = lerpTimeElapsed / lerpDuration;
				t = t * t * t * (t * (6f * t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
				foreach (RigLerp rigLerp in rigLerps) {
					float lerpValue = Mathf.Lerp(rigLerp.startValue, rigLerp.endValue, t);
					rigLerp.rig.weight = lerpValue;
				}

				lerpTimeElapsed += Time.deltaTime;
				yield return null;
			}
			// lerp never reaches endValue, that is why we have to set it manually
			foreach (RigLerp rigLerp in rigLerps) {
				rigLerp.rig.weight = rigLerp.endValue;
			}
		}

		/// <summary>
		/// Coroutine for smooth transform movements.
		/// </summary>
		/// <param name="startTarget"></param>
		/// <param name="startMapping"></param>
		/// <param name="endMapping"></param>
		/// <param name="duration"></param>
		/// <param name="alignTransforms"></param>
		/// <returns></returns>
		public IEnumerator LerpTransform(Transform startTarget, PosRotMapping startMapping, PosRotMapping endMapping, float duration, Action extraAction = null) {
			float time = 0;
			while (time < duration) {
				float t = time / duration;
				t = t * t * t * (t * (6f * t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
				startTarget.SetPositionAndRotation(Vector3.Lerp(startMapping.Position, endMapping.Position, t),
													Quaternion.Lerp(Quaternion.Euler(startMapping.Rotation), Quaternion.Euler(endMapping.Rotation), t));
				/*if (alignTransforms && m_transformAligner != null) {
					m_transformAligner.AlignTargetTransforms();
				}*/
				extraAction?.Invoke();
				time += Time.deltaTime;
				yield return null;
			}
			// lerp never reaches endValue, that is why we have to set it manually
			startTarget.SetPositionAndRotation(endMapping.Position, Quaternion.Euler(endMapping.Rotation));
			/*if (alignTransforms && m_transformAligner != null) {
				m_transformAligner.AlignTargetTransforms();
			}*/
			extraAction?.Invoke();
		}

		/// <summary>
		/// Coroutine for smooth movements of multiple transforms at once.
		/// </summary>
		/// <param name="targets">List of objects coupled with start and end mappings</param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public IEnumerator LerpTransforms(List<(Transform transform, PosRotMapping startMapping, PosRotMapping endMapping)> targets, float duration) {
			float time = 0;
			while (time < duration) {
				float t = time / duration;
				t = t * t * t * (t * (6f * t - 15f) + 10f); // https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
				foreach (var item in targets) {
					item.transform.SetPositionAndRotation(Vector3.Lerp(item.startMapping.Position, item.endMapping.Position, t),
															Quaternion.Lerp(Quaternion.Euler(item.startMapping.Rotation), Quaternion.Euler(item.endMapping.Rotation), t));

				}
				time += Time.deltaTime;
				yield return null;
			}
			// lerp never reaches endValue, that is why we have to set it manually
			foreach (var item in targets) {
				item.transform.SetPositionAndRotation(item.endMapping.Position, Quaternion.Euler(item.endMapping.Rotation));

			}
		}
	}
}