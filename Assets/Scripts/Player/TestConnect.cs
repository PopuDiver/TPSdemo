using System;
using Mirror;
using UnityEngine;

namespace Player {
    public class TestConnect : NetworkBehaviour {

        public static TestConnect instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
        }
    }
}