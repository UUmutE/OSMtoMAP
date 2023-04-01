using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MeshCreate : MonoBehaviour
{
    public GameObject roadPrefab; // Yol par�as� prefab�
    public GameObject yuvarlak; // Yol par�as� prefab�
    float roadWidth = 10f; // Yol par�as� geni�li�i
    //float roadHeight = 0.1f; // Yol par�as� y�ksekli�i

    public void CreateRoad(Vector3 startPoint, Vector3 endPoint)
    {
        Instantiate(yuvarlak, startPoint + new Vector3(0, 0.005f, 0), yuvarlak.transform.rotation);
        float distance = Vector3.Distance(startPoint, endPoint); // Noktalar aras�ndaki mesafe
        int roadPieces = Mathf.RoundToInt(distance / roadWidth); // Yol par�alar�n�n say�s�

        Vector3 direction = (endPoint - startPoint).normalized; // Noktalar aras�ndaki y�n vekt�r�

        // Yol par�alar�n� olu�tur
        for (int i = 0; i < roadPieces; i++)
        {
            Vector3 position = startPoint + direction * (i * roadWidth + roadWidth / 2f); // Yol par�as�n�n pozisyonu
            Quaternion rotation = Quaternion.LookRotation(direction); // Yol par�as�n�n rotasyonu

            // Yol par�as� prefab�n� kopyala ve pozisyonunu/rotasyonunu ayarla
            GameObject roadPiece = Instantiate(roadPrefab, position + new Vector3(0, 0.005f, 0), rotation);
        }
    }

    public void WithEarClipping(Vector3[] vertices)
    {
        // Vertex say�s�n� al
        int verticesCount = vertices.Length;

        // Vertexlerin 2D koordinatlar�n� tutacak dizi
        Vector2[] uv = new Vector2[verticesCount * 2];

        // ��genlerin dizisi
        int[] tringles;

        // Vertexlerin y�kseklik de�erlerini kald�rarak 2D koordinatlar�na �evir
        Vector2[] v2vertices = new Vector2[verticesCount];
        for (int i = 0; i < verticesCount; i++)
        {
            v2vertices[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        // Vertexlerin ��genle�tirilmesi
        List<int> abc = Triangulation.Tringulate(v2vertices.ToList());

        // E�er ��genle�tirme ba�ar�l� olduysa
        if (abc != null)
        {
            // Vertexlerin y�kseklik de�erlerini ekleyerek 3D koordinatlara �evir
            Vector3[] vertices2 = new Vector3[verticesCount * 2];
            Vector3 y�kseklik = new Vector3(0, 10, 0);
            for (int i = 0; i < verticesCount; i++)
            {
                vertices2[i] = vertices[i];
                vertices2[i + verticesCount] = vertices[i] + y�kseklik;
            }

            int a = abc.Count, b = verticesCount;
            List<int> temp = new List<int>();

            // �at� i�in ilk ��genleri tersten ekle
            for (int i = 0; i < a; i++)
            {
                temp.Add(abc[i] + b);
            }
            temp.Reverse();

            // Kenarlar�n ��gen listesine eklenmesi
            for (int i = 0; i < b - 1; i++)
            {
                temp.Add(i);
                temp.Add(i + b);
                temp.Add(i + b + 1);
                temp.Add(i + b + 1);
                temp.Add(i + 1);
                temp.Add(i);

                // D�zenlemeye ihtiya� duyan hata giderme alan�
                temp.Add(i);
                temp.Add(i + 1);
                temp.Add(i + b + 1);
                temp.Add(i + b + 1);
                temp.Add(i + b);
                temp.Add(i);
            }

            // Kenara ait son ��genlerin eklenmesi
            temp.Add(b - 1);
            temp.Add(b * 2 - 1);
            temp.Add(b);
            temp.Add(b);
            temp.Add(0);
            temp.Add(b - 1);

            // D�zenlemeye ihtiya� duyan hata giderme alan�
            temp.Add(b - 1);
            temp.Add(0);
            temp.Add(b);
            temp.Add(b);
            temp.Add(b * 2 - 1);
            temp.Add(b - 1);

            // ��genlerin dizisini birle�tir
            abc.AddRange(temp);
            tringles = abc.ToArray();

            // Mesh nesnesi olu�tur
            Mesh mesh = new Mesh();
            mesh.vertices = vertices2;
            mesh.triangles = tringles;
            mesh.uv = uv;

            // Oyun nesnesi olu�tur ve mesh'i ekle
            GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<Renderer>().material.color = Color.black;
        }
    }
}
