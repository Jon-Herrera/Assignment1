// 3-body Starter Code
// Fall 2026. IMDM 327
// Instructor. Myungin Lee
using UnityEngine;
using UnityEngine.Rendering;

public class ThreeB : MonoBehaviour
{
    private const float G = 50f; // Gravitational constant for this simulation, not the real-world value.
    BodyProperty[] bp;

    public Vector3 maxV = new Vector3(100f, 100f, 100f);
    float minimumDistance = 1f;
    private int numberOfSphere = 100;
    class BodyProperty // why struct?
    {                   // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
        public GameObject body;
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }


    void Start()
    {
        // Allocate an array to store each body's properties.
        bp = new BodyProperty[numberOfSphere];
        // Loop generating the gameobject and assign initial conditions (type, position, (mass/velocity/acceleration)
        for (int i = 0; i < numberOfSphere; i++)
        {
            // Our gameobjects are created here:
            bp[i] = new BodyProperty();
            bp[i].body = GameObject.CreatePrimitive(PrimitiveType.Sphere); // why sphere? try different options.
            // https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html

            // initial conditions
            float r = 100f;
            // position is (x,y,z). In this case, I want to plot them on the circle with r
            
            // ******** Fill in this part ********
            float theta = 2f * Mathf.PI / numberOfSphere * i;
            bp[i].body.transform.position = new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 180);
            // z = 180 places the bodies in front of a camera near the origin looking along +Z. Try other positions too.

            bp[i].velocity = Vector3.zero; // Try different initial condition
            bp[i].mass = 1; // Simplified. Try different initial condition


            // + This is just pretty trails
            TrailRenderer trailRenderer = bp[i].body.AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer.time = 100.0f;  // Duration of the trail
            trailRenderer.startWidth = 0.5f;  // Width of the trail at the start
            trailRenderer.endWidth = 0.1f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // Set the colour gradient along the trail.
            Gradient gradient = new Gradient();
            Color targetColor = Color.HSVToRGB((float)i / numberOfSphere, 1f, 1f);
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f), // (color, normalized position)
                    new GradientColorKey(targetColor, 0.8f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f), // (alpha, normalized position) 
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trailRenderer.colorGradient = gradient;

        }
    }

    void Update()
    {
        for (int i = 0; i< numberOfSphere; i++)
        {
            bp[i].acceleration = Vector3.zero; //set each acceleration to zero separately
        }
        for (int i = 0; i < numberOfSphere; i++)
        {
            for (int j = i + 1; j < numberOfSphere; j++)
            {
                Vector3 distance = bp[i].body.transform.position - bp[j].body.transform.position;//get distance between the two
                Vector3 gravity = CalculateGravity(distance, bp[i].mass, bp[j].mass); //calculate gravity between the two 
                bp[i].acceleration -= gravity / bp[i].mass; //calculate acceleration
                bp[j].acceleration += gravity / bp[j].mass; //calculate acceleration
                if (distance.magnitude < minimumDistance) //if two objects are closer than they should be
                {
                    bp[i].acceleration +=  gravity / bp[i].mass; // repel
                    bp[j].acceleration -= gravity / bp[j].mass; // repel
                }
            }
            
        }
        for (int i = 0; i < numberOfSphere; i++)
        {
            bp[i].velocity += bp[i].acceleration * Time.deltaTime;
            bp[i].velocity = Vector3.ClampMagnitude(bp[i].velocity, 20f);
            bp[i].body.transform.position += bp[i].velocity * Time.deltaTime;
        }
            

    }

    // Gravity Fuction to finish
    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity = Vector3.zero; // note this is also Vector3
        gravity = G * m1 * m2 / distanceVector.sqrMagnitude * distanceVector.normalized;                                // **** Fill in the function below. 
                                        // gravity = ****;
        return gravity;
    }
}

