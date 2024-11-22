using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HarmEntities : MonoBehaviour
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
                var otherTag = other.gameObject.tag;
                var harmInfo = tagsToAffect.Find(x => x.tag == otherTag);
                if(harmInfo.tag == "") continue;
                var damage = harmInfo.damage;
                var knockBack = harmInfo.knockback * (other.transform.position - transform.position).normalized;
                other.GetComponent<Harmable>()?.Harm(damage, knockBack);
            }
        }
    }
}
