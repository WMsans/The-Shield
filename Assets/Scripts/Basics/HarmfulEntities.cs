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
    }
    [Tooltip("List of colliders that can harm the player. Leave empty to affect all colliders")]
    [SerializeField] private List<Collider2D> harmingColliders;
    [SerializeField] private List<HarmableTag> tagsToAffect;
    [SerializeField] private UnityEvent<Vector2> onShielded;
    [SerializeField] private UnityEvent<Harmable> onHarmingOther;

    private void Start()
    {
        if (harmingColliders.Count <= 0)
        {
            harmingColliders = new List<Collider2D>(GetComponentsInChildren<Collider2D>());
        }
    }

    void FixedUpdate()
    {
        if (harmingColliders.Count <= 0) return;
        foreach (var collider1 in harmingColliders)
        {
            var results = new List<Collider2D>();
            collider1.OverlapCollider(new ContactFilter2D(), results);
            foreach (var other in results)
            {
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
                        HitShielded(knockBack * new Vector2(-1, -1));
                    }else
                    {
                        otherHarmable.Harm(damage, knockBack);
                        onHarmingOther.Invoke(otherHarmable);
                    }
                }
            }
        }
    }
    public void HitShielded(Vector2 knockback)
    {
        onShielded.Invoke(knockback);
    }
    
}
