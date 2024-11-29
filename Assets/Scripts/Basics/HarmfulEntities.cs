using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class HarmfulEntities : MonoBehaviour
{
    [Serializable]
    struct HarmableTag
    {
        public string tag;
        public float damage;
        public Vector2 knockback;
        public float stunDuration;
        public float ShakeAmount => damage * 0.03f;
    }
    [Tooltip("List of colliders that can harm the player. Leave empty to affect all colliders")]
    [SerializeField] private List<Collider2D> harmingColliders;
    [SerializeField] private List<HarmableTag> tagsToAffect;
    [SerializeField] private UnityEvent<Vector2> onShielded;
    [SerializeField] private UnityEvent<Harmable> onHarmingOther;
    private bool Stuned { get; set; }

    private void Start()
    {
        Stuned = false;
        if (harmingColliders.Count <= 0)
        {
            harmingColliders = new List<Collider2D>(GetComponentsInChildren<Collider2D>());
        }
    }

    private void FixedUpdate()
    {
        CheckForHarmable();
    }

    private void CheckForHarmable()
    {
        if (harmingColliders.Count <= 0) return;
        foreach (var collider1 in harmingColliders)
        {
            var results = new List<Collider2D>();
            collider1.OverlapCollider(new ContactFilter2D(), results);
            foreach (var other in results)
            {
                if(Stuned) return;
                if(other.gameObject == gameObject) continue;
                var otherTag = other.gameObject.tag;
                if(!tagsToAffect.Exists(x => x.tag == otherTag)) continue;
                var harmInfo = tagsToAffect.Find(x => x.tag == otherTag);
                var damage = harmInfo.damage;
                var knockBack = harmInfo.knockback * ((Vector2)(other.transform.position - transform.position)).normalized;
                var otherHarmable = other.GetComponent<Harmable>();
                if (otherHarmable)
                {
                    if (otherHarmable.Shielded)
                    {
                        HitShielded(damage,knockBack * new Vector2(-1, -1), otherHarmable);
                    }else
                    {
                        otherHarmable.Harm(damage, knockBack);
                        onHarmingOther.Invoke(otherHarmable);
                    }
                }
            }
        }
    }
    private void HitShielded(float damage, Vector2 knockback, Harmable other)
    {
        onShielded.Invoke(knockback);
        other.OnShielded(damage);
        var otherTag = tagsToAffect.Find(x => other.gameObject.CompareTag(x.tag));
        StartCoroutine(Stun(otherTag));
        // Screen shake
        ShakeCamera(otherTag.ShakeAmount);
        FrozeFrame(.1f);
    }

    private IEnumerator Stun(HarmableTag otherTag)
    {
        var duration = otherTag.stunDuration;
        Stuned = true;
        yield return new WaitForSeconds(duration);
        Stuned = false;
    }
    private void ShakeCamera(float amount)
    {
        CameraShake.Instance?.ShakeCamera(amount);
    }

    private void FrozeFrame(float duration)
    {
        TimeManager.Instance?.FrozenTime(duration, 0f);
    }
}
