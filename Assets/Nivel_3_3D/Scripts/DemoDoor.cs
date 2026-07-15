using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScifiOffice {
    [RequireComponent(typeof(Animator))]
    public class DemoDoor : MonoBehaviour {
        Animator anim;

        private void Start() {
            anim = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other) {
            if(anim != null && (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))) {
                anim.SetTrigger("Open");
            }
        }
    }
}
