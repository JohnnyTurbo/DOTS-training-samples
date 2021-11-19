﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public Mesh barMesh;
    public Material barMaterial;
    public Tornado[] tornados;

    public float expForce;
    public float breakResistance;
    [Range(0f, 1f)] public float damping;
    [Range(0f, 1f)] public float friction;

    Point[] points;
    Bar[] bars;
    public int pointCount;

    float tornadoFader = 0f;

    Matrix4x4[][] matrices;
    MaterialPropertyBlock[] matProps;

    const int instancesPerBatch = 1023;

    private void Awake()
    {
        Time.timeScale = 0f;
    }

    void Start()
    {
        Generate();
    }

    public static float TornadoSway(float y)
    {
        return TornadoSway(y, Time.time);
    }

    public static float TornadoSway(float y, float time)
    {
        return Mathf.Sin(y / 5f + time / 4f) * 3f;
    }

    void Generate()
    {
        // buildings
        var pointsList = new List<Point>();
        for (int i = 0; i < 100; i++)
        {
            int height = Random.Range(6, 15);
            Vector3 pos = new Vector3(Random.Range(-45f, 45f), 0f, Random.Range(-45f, 45f));
            float spacing = 2f;
            for (int j = 0; j < height; j++)
            {
                Point point = new Point();
                point.x = pos.x + spacing;
                point.y = j * spacing;
                point.z = pos.z - spacing;
                point.oldX = point.x;
                point.oldY = point.y;
                point.oldZ = point.z;
                if (j == 0)
                {
                    point.anchor = true;
                }
                pointsList.Add(point);
                point = new Point();
                point.x = pos.x - spacing;
                point.y = j * spacing;
                point.z = pos.z - spacing;
                point.oldX = point.x;
                point.oldY = point.y;
                point.oldZ = point.z;
                if (j == 0)
                {
                    point.anchor = true;
                }
                pointsList.Add(point);
                point = new Point();
                point.x = pos.x + 0f;
                point.y = j * spacing;
                point.z = pos.z + spacing;
                point.oldX = point.x;
                point.oldY = point.y;
                point.oldZ = point.z;
                if (j == 0)
                {
                    point.anchor = true;
                }
                pointsList.Add(point);
            }
        }

        // ground details
        for (int i = 0; i < 0; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-55f, 55f), 0f, Random.Range(-55f, 55f));
            Point point = new Point();
            point.x = pos.x + Random.Range(-.2f, -.1f);
            point.y = pos.y + Random.Range(0f, 3f);
            point.z = pos.z + Random.Range(.1f, .2f);
            point.oldX = point.x;
            point.oldY = point.y;
            point.oldZ = point.z;
            pointsList.Add(point);

            point = new Point();
            point.x = pos.x + Random.Range(.2f, .1f);
            point.y = pos.y + Random.Range(0f, .2f);
            point.z = pos.z + Random.Range(-.1f, -.2f);
            point.oldX = point.x;
            point.oldY = point.y;
            point.oldZ = point.z;
            if (Random.value < .1f)
            {
                point.anchor = true;
            }
            pointsList.Add(point);
        }

        int batch = 0;
        var barsList = new List<Bar>();
        var matricesList = new List<List<Matrix4x4>> { new List<Matrix4x4>() };
        for (int i = 0; i < pointsList.Count; i++)
        {
            for (int j = i + 1; j < pointsList.Count; j++)
            {
                Bar bar = new Bar();
                bar.AssignPoints(pointsList[i], pointsList[j]);
                if (bar.length < 5f && bar.length > .2f)
                {
                    bar.point1.neighborCount++;
                    bar.point2.neighborCount++;

                    barsList.Add(bar);
                    matricesList[batch].Add(bar.matrix);
                    if (matricesList[batch].Count == instancesPerBatch)
                    {
                        batch++;
                        matricesList.Add(new List<Matrix4x4>());
                    }
                }
            }
        }
        
        pointCount = 0;
        points = new Point[barsList.Count * 2];
        for (int i = 0; i < pointsList.Count; i++)
        {
            if (pointsList[i].neighborCount > 0)
            {
                points[pointCount] = pointsList[i];
                pointCount++;
            }
        }
        
        Debug.Log(pointCount + " points, room for " + points.Length + " (" + barsList.Count + " bars)");

        bars = barsList.ToArray();

        matrices = new Matrix4x4[matricesList.Count][];
        for (int i = 0; i < matrices.Length; i++)
            matrices[i] = matricesList[i].ToArray();

        matProps = new MaterialPropertyBlock[barsList.Count];
        var colors = new Vector4[instancesPerBatch];
        for (int i = 0; i < barsList.Count; i++)
        {
            colors[i % instancesPerBatch] = barsList[i].color;
            if ((i + 1) % instancesPerBatch == 0 || i == barsList.Count - 1)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetVectorArray("_Color", colors);
                matProps[i / instancesPerBatch] = block;
            }
        }

        pointsList = null;
        barsList = null;
        matricesList = null;
        System.GC.Collect();
        Time.timeScale = 1f;
    }

    void FixedUpdate()
    {
        tornadoFader = Mathf.Clamp01(tornadoFader + Time.deltaTime / 10f);

        foreach (var tornado in tornados)
        {
            // if (!tornado.simulate)
            //     continue;

            float invDamping = 1f - damping;
            for (int i = 0; i < pointCount; i++)
            {
                Point point = points[i];
                if (point.anchor == false)
                {
                    float startX = point.x;
                    float startY = point.y;
                    float startZ = point.z;

                    point.oldY += .01f;

                    // tornado force
                    if (tornado.simulate)
                    {
                        float tdx = tornado.x + TornadoSway(point.y) - point.x;
                        float tdz = tornado.y - point.z;
                        float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
                        tdx /= tornadoDist;
                        tdz /= tornadoDist;
                        if (tornadoDist < tornado.maxForceDist)
                        {
                            float force = (1f - tornadoDist / tornado.maxForceDist);
                            float yFader = Mathf.Clamp01(1f - point.y / tornado.height);
                            force *= tornadoFader * tornado.force * Random.Range(-.3f, 1.3f);
                            float forceY = tornado.upForce;
                            point.oldY -= forceY * force;
                            float forceX = -tdz + tdx * tornado.inwardForce * yFader;
                            float forceZ = tdx + tdz * tornado.inwardForce * yFader;
                            point.oldX -= forceX * force;
                            point.oldZ -= forceZ * force;
                        }
                    }

                    point.x += (point.x - point.oldX) * invDamping;
                    point.y += (point.y - point.oldY) * invDamping;
                    point.z += (point.z - point.oldZ) * invDamping;

                    point.oldX = startX;
                    point.oldY = startY;
                    point.oldZ = startZ;
                    if (point.y < 0f)
                    {
                        point.y = 0f;
                        point.oldY = -point.oldY;
                        point.oldX += (point.x - point.oldX) * friction;
                        point.oldZ += (point.z - point.oldZ) * friction;
                    }
                }
            }

            for (int i = 0; i < bars.Length; i++)
            {
                Bar bar = bars[i];

                Point point1 = bar.point1;
                Point point2 = bar.point2;

                float dx = point2.x - point1.x;
                float dy = point2.y - point1.y;
                float dz = point2.z - point1.z;

                float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
                float extraDist = dist - bar.length;

                float pushX = (dx / dist * extraDist) * .5f;
                float pushY = (dy / dist * extraDist) * .5f;
                float pushZ = (dz / dist * extraDist) * .5f;

                if (point1.anchor == false && point2.anchor == false)
                {
                    point1.x += pushX;
                    point1.y += pushY;
                    point1.z += pushZ;
                    point2.x -= pushX;
                    point2.y -= pushY;
                    point2.z -= pushZ;
                }
                else if (point1.anchor)
                {
                    point2.x -= pushX * 2f;
                    point2.y -= pushY * 2f;
                    point2.z -= pushZ * 2f;
                }
                else if (point2.anchor)
                {
                    point1.x += pushX * 2f;
                    point1.y += pushY * 2f;
                    point1.z += pushZ * 2f;
                }

                if (dx / dist * bar.oldDX + dy / dist * bar.oldDY + dz / dist * bar.oldDZ < .99f)
                {
                    // bar has rotated: expensive full-matrix computation
                    bar.matrix = Matrix4x4.TRS(new Vector3((point1.x + point2.x) * .5f, (point1.y + point2.y) * .5f, (point1.z + point2.z) * .5f),
                                           Quaternion.LookRotation(new Vector3(dx, dy, dz)),
                                           new Vector3(bar.thickness, bar.thickness, bar.length));
                    bar.oldDX = dx / dist;
                    bar.oldDY = dy / dist;
                    bar.oldDZ = dz / dist;
                }
                else
                {
                    // bar hasn't rotated: only update the position elements
                    Matrix4x4 matrix = bar.matrix;
                    matrix.m03 = (point1.x + point2.x) * .5f;
                    matrix.m13 = (point1.y + point2.y) * .5f;
                    matrix.m23 = (point1.z + point2.z) * .5f;
                    bar.matrix = matrix;
                }

                if (Mathf.Abs(extraDist) > breakResistance)
                {
                    if (point2.neighborCount > 1)
                    {
                        point2.neighborCount--;
                        Point newPoint = new Point();
                        newPoint.CopyFrom(point2);
                        newPoint.neighborCount = 1;
                        points[pointCount] = newPoint;
                        bar.point2 = newPoint;
                        pointCount++;
                    }
                    else if (point1.neighborCount > 1)
                    {
                        point1.neighborCount--;
                        Point newPoint = new Point();
                        newPoint.CopyFrom(point1);
                        newPoint.neighborCount = 1;
                        points[pointCount] = newPoint;
                        bar.point1 = newPoint;
                        pointCount++;
                    }
                }

                bar.minX = Mathf.Min(point1.x, point2.x);
                bar.maxX = Mathf.Max(point1.x, point2.x);
                bar.minY = Mathf.Min(point1.y, point2.y);
                bar.maxY = Mathf.Max(point1.y, point2.y);
                bar.minZ = Mathf.Min(point1.z, point2.z);
                bar.maxZ = Mathf.Max(point1.z, point2.z);

                matrices[i / instancesPerBatch][i % instancesPerBatch] = bar.matrix;
            }
        }
    }

    private void Update()
    {
        if (matrices == null)
            return;
        
        for (int i = 0; i < matrices.Length; i++)
            Graphics.DrawMeshInstanced(barMesh, 0, barMaterial, matrices[i], matrices[i].Length, matProps[i]);
    }
}
