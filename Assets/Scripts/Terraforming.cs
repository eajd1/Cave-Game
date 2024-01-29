using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terraforming : MonoBehaviour
{
    public Transform playerCamera;
    public WorldTerrain worldTerrain;
    public int terraformingRange;

    public int layer;
    public float miningCooldown, miningRadius;
    [Range(0, 1)]
    public float miningPercentage;
    float nextMine = 0f;

    public float fillCooldown, fillRadius;
    [Range(0, 1)]
    public float fillingPercentage;
    float nextFill = 0f;

    public float bombCooldown, bombRadius;
    [Range(0, 1)]
    public float bombPercentage;
    float nextBomb = 0f;

    public KeyCode flatten;
    public KeyCode bombKey;
    public Color32 paintColour;

    bool start = true;

    void LateUpdate()
    {
        if (start)
        {
            worldTerrain.EditTerrain(playerCamera.position, 8, 1f, SphereBrush(8f, 1f));
            start = false;
        }
    }

    void FixedUpdate()
    {
        int layerMask = 1 << layer;
        if (Input.GetButton("Fire1") && Time.time > nextMine)
        {
            nextMine = Time.time + miningCooldown;
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position + Vector3.forward * 0.4f, playerCamera.TransformDirection(Vector3.forward), out hit, terraformingRange, layerMask))
            {
                if (Input.GetKey(flatten))
                {
                    worldTerrain.EditTerrain(hit.point, miningRadius, miningPercentage * 2, HemiSphereBrush(miningRadius, miningPercentage * 2));
                }
                else
                {
                    worldTerrain.EditTerrain(hit.point, miningRadius, miningPercentage, SphereBrush(miningRadius, miningPercentage));
                }
            }
        }
        if (Input.GetButton("Fire2") && Time.time > nextFill)
        {
            nextMine = Time.time + fillCooldown;
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position + Vector3.forward * 0.4f, playerCamera.TransformDirection(Vector3.forward), out hit, terraformingRange, layerMask))
            {
                if (Input.GetKey(flatten))
                {
                    worldTerrain.PaintTerrain(hit, paintColour);
                }
                else
                {
                    worldTerrain.EditTerrain(hit.point, fillRadius, -fillingPercentage, SphereBrush(fillRadius, fillingPercentage));
                }
            }
        }
        if (Input.GetKey(bombKey) && Time.time > nextBomb)
        {
            nextBomb = Time.time + bombCooldown;
            if (Input.GetKey(flatten))
            {
                worldTerrain.EditTerrain(playerCamera.position + (Vector3.down * 1.8f), bombRadius, bombPercentage, HemiSphereBrush(bombRadius, bombPercentage));
            }
            else
            {
                worldTerrain.EditTerrain(playerCamera.position, bombRadius, bombPercentage, SphereBrush(bombRadius, bombPercentage));
            }
        }
    }

    Texture3D SphereBrush(float radius, float amount)
    {
        Texture3D texture = new Texture3D(Mathf.CeilToInt(radius) * 2, Mathf.CeilToInt(radius) * 2, Mathf.CeilToInt(radius) * 2, TextureFormat.Alpha8, false);
        Vector3 mid = new Vector3(radius, radius, radius);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int z = 0; z < texture.depth; z++)
                {
                    float dist = Vector3.Distance(mid, new Vector3(x, y, z));
                    if (dist <= radius)
                    {
                        texture.SetPixel(x, y, z, new Color(0f, 0f, 0f, amount * Mathf.InverseLerp(0, radius, radius - dist)));
                    }
                    else
                    {
                        texture.SetPixel(x, y, z, new Color(0f, 0f, 0f, 0f));
                    }
                }
            }
        }
        return texture;
    }

    Texture3D HemiSphereBrush(float radius, float amount)
    {
        Texture3D texture = SphereBrush(radius, amount);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int z = 0; z < texture.depth; z++)
                {
                    if (y < radius)
                    {
                        texture.SetPixel(x, y, z, new Color(0f, 0f, 0f, -amount));
                    }
                }
            }
        }
        return texture;
    }
}
