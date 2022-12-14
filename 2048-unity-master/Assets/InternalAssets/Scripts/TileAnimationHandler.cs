using UnityEngine;
using System.Collections;

public sealed class TileAnimationHandler : MonoBehaviour
{
    public float scaleSpeed;
    public float growSize;

    private Transform _transform;
    private Vector3 growVector;

    private void Start()
    {
        _transform = transform;

        growVector = new Vector3(growSize, growSize, 0f);
    }

    public void AnimateEntry() => StartCoroutine(nameof(AnimationEntry));

    public void AnimateUpgrade() => StartCoroutine(nameof(AnimationUpgrade));

    private IEnumerator AnimationEntry()
    {
        while (_transform == null) yield return null;

        _transform.localScale = new Vector3(1f, 1f, 1f);

        while (_transform.localScale.x < 1f)
        {
            _transform.localScale = Vector3.MoveTowards(_transform.localScale, Vector3.one, scaleSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator AnimationUpgrade()
    {
        while (_transform == null) yield return null;

        while (_transform.localScale.x < 1f + growSize)
        {
            _transform.localScale = Vector3.MoveTowards(_transform.localScale, Vector3.one + growVector, scaleSpeed * Time.deltaTime);

            yield return null;
        }

        while (_transform.localScale.x > 1f)
        {
            _transform.localScale = Vector3.MoveTowards(_transform.localScale, Vector3.one, scaleSpeed * Time.deltaTime);

            yield return null;
        }
    }
}