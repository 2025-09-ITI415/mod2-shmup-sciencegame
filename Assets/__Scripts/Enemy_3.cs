using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy_3 : Enemy
{
    // Enemy_3 also extends the Enemy class
    [Header("Enemy_3 Inscribed Fields")]
    public float lifeTime = 5;
    public Vector2 midpointYRange = new Vector2(1.5f, 3);
    [Tooltip("If true, the Bezier points & path are drawn in the Scene pane.")]
    public bool drawDebugInfo = true;


    [Header("Enemy_3 Private Fields")]
    [SerializeField]
    private Vector3[] points;  // The three points for the Bezier curve
    [SerializeField]
    private float birthTime;

    // Again, Start works well because it is not used in the Enemy superclass
    void Start()
    {
        points = new Vector3[3]; // Initialize points

        // The start position has already been set by Main.SpawnEnemy()
        points[0] = pos;

        // Set xMin and xMax the same way that Main.SpawnEnemy() does
        float xMin = -bndCheck.camWidth + bndCheck.radius;
        float xMax = bndCheck.camWidth - bndCheck.radius;

        // Pick a random middle position in the bottom half of the screen
        points[1] = Vector3.zero;
        points[1].x = Random.Range(xMin, xMax);
        float midYMult = Random.Range(midpointYRange[0], midpointYRange[1]);        // a
        points[1].y = -bndCheck.camHeight * midYMult;                               // a

        // Pick a random final position above the top of the screen
        points[2] = Vector3.zero;
        points[2].y = pos.y;
        points[2].x = Random.Range(xMin, xMax);

        // Set the birthTime to the current time
        birthTime = Time.time;

        if (drawDebugInfo) DrawDebug();                                           // b
    }

    public override void Move()
    {
        // Bezier curves work based on a u value between 0 & 1
        float u = (Time.time - birthTime) / lifeTime;

        if (u > 1)
        {
            // This Enemy_3 has finished its life
            Destroy(this.gameObject);
            return;
        }

        transform.rotation = Quaternion.Euler(u * 180, 0, 0);                     // c

        // Interpolate the three Bezier curve points
        u = u - 0.1f * Mathf.Sin(u * Mathf.PI * 2);
        pos = Utils.Bezier(u, points);

        // Enemy_3 does not call base.Move()
    }

    void DrawDebug()
    {                                                              // e
                                                                   // Draw the three points
        Debug.DrawLine(points[0], points[1], Color.cyan, lifeTime);               // f
        Debug.DrawLine(points[1], points[2], Color.yellow, lifeTime);

        // Draw the Bezier Curve
        float numSections = 20;
        Vector3 prevPoint = points[0];                                              // g
        Color col;
        Vector3 pt;
        for (int i = 1; i < numSections; i++)
        {                                   // h
            float u = i / numSections;
            pt = Utils.Bezier(u, points);
            col = Color.Lerp(Color.cyan, Color.yellow, u);
            Debug.DrawLine(prevPoint, pt, col, lifeTime);                         // i
            prevPoint = pt;
        }
    }
}