using UnityEngine; 

namespace Mod
{
    public class Mod
    {
        public static void Main()
        {
            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Metal Cube"),
                    NameOverride = "APC",
                    DescriptionOverride = "I don't know what APC stands for.",
                    CategoryOverride = ModAPI.FindCategory("Vehicles"),
                    ThumbnailOverride = ModAPI.LoadSprite("preview.png"),
                    AfterSpawn = (Instance) =>
                    {
                        Instance.GetComponent<SpriteRenderer>().sprite=ModAPI.LoadSprite("apcBody.png");
                        float direction = Instance.transform.localScale.x;
                        GameObject.Destroy(Instance.GetComponent<BoxCollider2D>());
                        Instance.gameObject.FixColliders();
                        
                        Instance.GetComponent<Rigidbody2D>().mass=5000f;

                        GameObject gun = GameObject.Instantiate(ModAPI.FindSpawnable("G1 Submachine Gun").Prefab, Instance.transform.position, Quaternion.identity);
                        gun.GetComponent<SpriteRenderer>().sprite=ModAPI.LoadSprite("barrel.png");
                        foreach(Collider2D c in gun.GetComponents<Collider2D>()){GameObject.Destroy(c);}
                        gun.AddComponent<BoxCollider2D>();
                        gun.transform.position+=new Vector3(direction>0?2.2f:-1.2f, 0.8f);

                        gun.GetComponent<Rigidbody2D>().angularDrag=10000f;
                        gun.GetComponent<Rigidbody2D>().gravityScale=0f;
                        HingeJoint2D hj = gun.AddComponent<HingeJoint2D>();
                        hj.connectedBody=Instance.GetComponent<Rigidbody2D>();
                        hj.enableCollision=false;
                        hj.anchor=new Vector2(-0.5f, 0f);
                        JointAngleLimits2D ja = new JointAngleLimits2D();
                        ja.min=direction>0?-70:170;
                        ja.max=direction>0?10:250;
                        hj.limits=ja;

                        GameObject w=ModAPI.FindSpawnable("Wheel").Prefab;
                        Sprite ws=ModAPI.LoadSprite("wheel.png");
                        float[] wps={-2.65f, -1, 1, 2.65f};

                        CarBehaviour car = Instance.AddComponent<CarBehaviour>();
                        car.WheelJoints = new WheelJoint2D[4];

                        APC apc = Instance.AddComponent<APC>();
                        apc.objects=new GameObject[5];

                        for(int i=0;i<4;i++)
                        {
                            GameObject wheel = GameObject.Instantiate(w, Instance.transform.position+new Vector3(wps[i], -1.5f, 0f), Quaternion.identity);
                            wheel.GetComponent<Rigidbody2D>().mass=100f;
                            wheel.GetComponent<SpriteRenderer>().sprite=ws;
                            wheel.GetComponent<SpriteRenderer>().sortingOrder=1;
                            wheel.GetComponent<CircleCollider2D>().radius=0.7f;
                            WheelJoint2D wj = Instance.AddComponent<WheelJoint2D>();
                            wj.connectedBody=wheel.GetComponent<Rigidbody2D>();
                            wj.enableCollision=false;
                            wj.anchor=new Vector3(wps[i], -1f, 0f);
                            wj.autoConfigureConnectedAnchor=true;
                            JointSuspension2D js = wj.suspension;
                            js.dampingRatio=0.75f;
                            js.frequency=3f;
                            wj.suspension=js;
                            wj.breakForce=7500f;
                            car.WheelJoints[i]=wj;
                            apc.objects[i]=wheel;
                        }

                        apc.objects[4]=gun;

                        car.MotorSpeed=-700f;
                        car.Activated=false;
                        car.WaterProof=true;
                        car.EngineStartup=ModAPI.LoadSound("start.mp3");
                        car.EngineShutoff=ModAPI.LoadSound("stop.mp3");
                        car.Phys=Instance.GetComponent<PhysicalBehaviour>();
                        car.IsBrakeEngaged=true;

                        apc.car=car;
                        apc.source=Instance.AddComponent<AudioSource>();//the car behaviour's loop property does not loop on its own
                        apc.source.clip=ModAPI.LoadSound("loop.mp3");
                        apc.source.loop=true;
                        apc.source.minDistance=1f;
                        apc.source.maxDistance=10f;
                    }
                }
            );
        }
    }

    public class APC : MonoBehaviour
    {
        public GameObject[] objects;

        public CarBehaviour car;

        public AudioSource source;

        private bool playing = false;

        void Update()
        {
            if(car!=null && car.Activated)
            {
                if(!playing)
                {
                    source.Play();
                    playing=true;
                }
            }
            else{source.Stop();playing=false;}
        }
        
        void OnDestroy()
        {
            foreach(GameObject o in objects){GameObject.Destroy(o);}
        }
    }
}