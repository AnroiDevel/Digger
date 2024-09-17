using UnityEngine;

namespace MultiTool
{
    public class PlayerAnimation : MonoBehaviour
    {
        private const string JumpTriggerName = "Jump";
        private const string WalkTriggerName = "IsWalk";
        private const string StartTeleport = "StartTeleport";
        private const string EndTeleport = "EndTeleport";
        private const string StartShapeshift = "StartShapeshift";
        private const string EndShapeshift = "EndShapeshift";
        private const string MineTriggerName = "Mine";
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetJump()
        {
            _animator.SetTrigger(JumpTriggerName);
        }

        public void SetWalk(bool isWalking)
        {
            _animator.SetBool(WalkTriggerName, isWalking);
        }

        internal void Teleport(bool isTeleport)
        {
            var triggerName = isTeleport ? StartTeleport : EndTeleport;
            _animator.SetTrigger(triggerName);
        }

        internal void Shapeshift(bool isShapeshift)
        {
            var triggerName = isShapeshift ? StartShapeshift : EndShapeshift;
            _animator.SetTrigger(triggerName);
        }

        internal void Mine()
        {
            _animator.SetTrigger(MineTriggerName);
        }
    }
}
