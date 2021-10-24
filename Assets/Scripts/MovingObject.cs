using System.Collections;

using UnityEngine;

namespace Completed
{
    public abstract class MovingObject : MonoBehaviour
    {
        public float moveTime = 0.1f; // time takes object to move in seconds
        public LayerMask blockingLayer;

        private BoxCollider2D boxCollider;
        private Rigidbody2D rigidBody;
        private float inverseMoveTime; // use reciprocal of moveTime for multiplication over division for efficiency
        private bool isMoving = false; // used to prevent multiple inputs when unit is already moving

        protected virtual void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            rigidBody = GetComponent<Rigidbody2D>();
            inverseMoveTime = 1f / moveTime;
        }

        /// <summary>
        /// Define MovingObject behavior when it hits object of generic type T
        /// </summary>
        protected abstract void OnCannotMove<T>(T component) where T : Component;

        /// <summary>
        /// Either successfully perform Move or pass the blocker object to OnCannotMove
        /// </summary>
        protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component
        {
            // hit will store whatever our linecast hits when Move is called
            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit);

            // if nothing was hit by linecast, move was successful
            if (hit.transform == null)
            {
                return;
            }

            // object has hit some object of generic type T
            T hitComponent = hit.transform.GetComponent<T>();
            if (!canMove && hitComponent != null)
            {
                OnCannotMove(hitComponent);
            }
        }

        /// <summary>
        /// Return true if move is possible (has no blockers and not already moving) and perform the move, else return false
        /// </summary>
        protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            Vector2 startPosition = transform.position;
            Vector2 endPosition = startPosition + new Vector2(xDir, yDir);

            // disable boxCollider so that linecast doesn't hit this object's own collider
            boxCollider.enabled = false;
            // cast a line from start point to end point, checking collision on blockingLayer
            hit = Physics2D.Linecast(startPosition, endPosition, blockingLayer);
            // re-enable boxCollider after linecast
            boxCollider.enabled = true;

            if (hit.transform == null && !isMoving)
            {
                StartCoroutine(SmoothMovement(endPosition));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Coroutine for moving units to end position
        /// </summary>
        protected IEnumerator SmoothMovement(Vector3 endPosition)
        {
            isMoving = true;

            // use square distance for efficiency
            float squareDistance = (transform.position - endPosition).sqrMagnitude;

            while (squareDistance > float.Epsilon)
            {
                // move to new position proportionally closer to the end position, based on the moveTime
                Vector3 nextPosition = Vector3.MoveTowards(rigidBody.position, endPosition, inverseMoveTime * Time.deltaTime);
                rigidBody.MovePosition(nextPosition);

                // recalculate the remaining distance after moving
                squareDistance = (transform.position - endPosition).sqrMagnitude;

                // return and loop until distance is close enough to zero to end the function
                yield return null;
            }

            rigidBody.MovePosition(endPosition);

            isMoving = false;
        }
    }
}
