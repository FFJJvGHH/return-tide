using UnityEngine;
namespace ReturnTide {
    [RequireComponent(typeof(CharacterController))]
    public class TidePlayer : MonoBehaviour {
        public TideSettings settings;
        public Transform visual, slash;
        public TrailRenderer dashTrail;
        public bool controllable;
        public int Health { get; private set; }
        public float DashReady => Mathf.Clamp01(1-(nextDash-Time.time)/settings.dashCooldown);
        CharacterController motor;
        Vector3 direction=Vector3.forward;
        float nextDash,dashUntil,nextAttack,invulnerableUntil,slashUntil;
        Vector3 visualOrigin;
        void Awake() { motor=GetComponent<CharacterController>(); Health=settings.maxHealth; visualOrigin=visual.localPosition; }
        void Update() {
            if(!controllable) return;
            Vector3 input=new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
            input=Vector3.ClampMagnitude(input,1);
            if(input.sqrMagnitude>.01f) direction=input;
            if(Input.GetKeyDown(KeyCode.Space)&&Time.time>=nextDash) { dashUntil=Time.time+settings.dashDuration;nextDash=Time.time+settings.dashCooldown;TideGame.Instance.Sound(520,.10f); }
            bool dashing=Time.time<dashUntil;
            motor.Move(((dashing?direction*settings.dashSpeed:input*settings.moveSpeed)+Vector3.down*4)*Time.deltaTime);
            if(dashTrail) dashTrail.emitting=dashing;
            visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(direction),Time.deltaTime*14);
            visual.localPosition=visualOrigin+Vector3.up*(input.magnitude*Mathf.Abs(Mathf.Sin(Time.time*11))*.07f);
            if(Input.GetMouseButtonDown(0)||Input.GetKeyDown(KeyCode.J)) Attack();
            if(slash) { slash.gameObject.SetActive(Time.time<slashUntil); slash.localRotation=Quaternion.Euler(0,(1-(slashUntil-Time.time)/.23f)*150-75,0); }
            if(transform.position.y < -5) TideGame.Instance.Respawn();
        }
        public void Attack() {
            if(Time.time<nextAttack)return; nextAttack=Time.time+settings.attackCooldown;slashUntil=Time.time+.23f;
            TideGame.Instance.Sound(820,.07f);
            foreach(var enemy in FindObjectsOfType<TideEnemy>()) if(Vector3.Distance(transform.position,enemy.transform.position)<settings.attackRadius) enemy.Hit(direction);
        }
        public void Damage(int amount) {
            if(!controllable||Time.time<invulnerableUntil||Time.time<dashUntil)return;
            Health-=amount;invulnerableUntil=Time.time+1.2f;
            TideGame.Instance.hud.Flash();TideGame.Instance.Sound(130,.17f);
            if(Health<=0) TideGame.Instance.Respawn();
        }
        public void Heal() { Health=settings.maxHealth; }
        public void Teleport(Vector3 p) { if(!motor)motor=GetComponent<CharacterController>();motor.enabled=false;transform.position=p;motor.enabled=true; }
    }
}
