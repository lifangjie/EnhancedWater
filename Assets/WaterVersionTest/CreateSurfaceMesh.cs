using UnityEngine;

namespace WaterVersionTest {
    public class CreateSurfaceMesh {
        private int _gMeshDim = 128;
        private int _gLods = 0;

        struct QuadRenderParam {
            public int NumInnerVerts,
                NumInnerFaces,
                InnerStartIndex,
                NumBoundaryVerts,
                NumBoundaryFaces,
                BoundaryStartIndex;
        };

        private readonly QuadRenderParam[,,,,] _gMeshPatterns = new QuadRenderParam[9, 3, 3, 3, 3];

        struct Rect {
            public int Left, Top, Right, Bottom;

            public Rect(int left, int top, int right, int bottom) {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        private int MESH_INDEX_2D(int x, int y, Rect vertRect) {
            return (((y) + vertRect.Bottom) * (_gMeshDim + 1) + (x) + vertRect.Left);
        }

        int GenerateBoundaryMesh(int leftDegree, int rightDegree, int bottomDegree, int topDegree,
            Rect vertRect, int[] output, int offset) {
            // Triangle list for bottom boundary
            int i, j;
            int counter = 0;
            int width = vertRect.Right - vertRect.Left;

            if (bottomDegree > 0) {
                int bStep = width / bottomDegree;

                for (i = 0; i < width; i += bStep) {
                    output[offset + counter++] = MESH_INDEX_2D(i, 0, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(i + bStep / 2, 1, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(i + bStep, 0, vertRect);

                    for (j = 0; j < bStep / 2; j++) {
                        if (i == 0 && j == 0 && leftDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(i, 0, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j, 1, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j + 1, 1, vertRect);
                    }

                    for (j = bStep / 2; j < bStep; j++) {
                        if (i == width - bStep && j == bStep - 1 && rightDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(i + bStep, 0, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j, 1, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j + 1, 1, vertRect);
                    }
                }
            }

            // Right boundary
            int height = vertRect.Top - vertRect.Bottom;

            if (rightDegree > 0) {
                int rStep = height / rightDegree;

                for (i = 0; i < height; i += rStep) {
                    output[offset + counter++] = MESH_INDEX_2D(width, i, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(width - 1, i + rStep / 2, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(width, i + rStep, vertRect);

                    for (j = 0; j < rStep / 2; j++) {
                        if (i == 0 && j == 0 && bottomDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(width, i, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(width - 1, i + j, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(width - 1, i + j + 1, vertRect);
                    }

                    for (j = rStep / 2; j < rStep; j++) {
                        if (i == height - rStep && j == rStep - 1 && topDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(width, i + rStep, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(width - 1, i + j, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(width - 1, i + j + 1, vertRect);
                    }
                }
            }

            // Top boundary
            if (topDegree > 0) {
                int tStep = width / topDegree;

                for (i = 0; i < width; i += tStep) {
                    output[offset + counter++] = MESH_INDEX_2D(i, height, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(i + tStep / 2, height - 1, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(i + tStep, height, vertRect);

                    for (j = 0; j < tStep / 2; j++) {
                        if (i == 0 && j == 0 && leftDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(i, height, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j, height - 1, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j + 1, height - 1, vertRect);
                    }

                    for (j = tStep / 2; j < tStep; j++) {
                        if (i == width - tStep && j == tStep - 1 && rightDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(i + tStep, height, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j, height - 1, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(i + j + 1, height - 1, vertRect);
                    }
                }
            }

            // Left boundary
            if (leftDegree > 0) {
                int lStep = height / leftDegree;

                for (i = 0; i < height; i += lStep) {
                    output[offset + counter++] = MESH_INDEX_2D(0, i, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(1, i + lStep / 2, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(0, i + lStep, vertRect);

                    for (j = 0; j < lStep / 2; j++) {
                        if (i == 0 && j == 0 && bottomDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(0, i, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(1, i + j, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(1, i + j + 1, vertRect);
                    }

                    for (j = lStep / 2; j < lStep; j++) {
                        if (i == height - lStep && j == lStep - 1 && topDegree > 0)
                            continue;

                        output[offset + counter++] = MESH_INDEX_2D(0, i + lStep, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(1, i + j, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(1, i + j + 1, vertRect);
                    }
                }
            }

            return counter;
        }

        // Generate boundary mesh for a patch. Return the number of generated indices
        int GenerateInnerMesh(Rect vertRect, int[] output, int offset) {
            int i, j;
            int counter = 0;
            int width = vertRect.Right - vertRect.Left;
            int height = vertRect.Top - vertRect.Bottom;

            bool reverse = false;
            for (i = 0; i < height; i++) {
                if (reverse == false) {
                    output[offset + counter++] = MESH_INDEX_2D(0, i, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(0, i + 1, vertRect);
                    for (j = 0; j < width; j++) {
                        output[offset + counter++] = MESH_INDEX_2D(j + 1, i, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(j + 1, i + 1, vertRect);
                    }
                } else {
                    output[offset + counter++] = MESH_INDEX_2D(width, i, vertRect);
                    output[offset + counter++] = MESH_INDEX_2D(width, i + 1, vertRect);
                    for (j = width - 1; j >= 0; j--) {
                        output[offset + counter++] = MESH_INDEX_2D(j, i, vertRect);
                        output[offset + counter++] = MESH_INDEX_2D(j, i + 1, vertRect);
                    }
                }

                reverse = !reverse;
            }

            return counter;
        }


        //void Create(ID3D11Device* pd3DDevice) {
        public void Create() {
            // --------------------------------- Vertex Buffer -------------------------------
            int numVerts = (_gMeshDim + 1) * (_gMeshDim + 1);
            Vector2[] pV = new Vector2[numVerts];

            int i, j;
            for (i = 0; i <= _gMeshDim; i++) {
                for (j = 0; j <= _gMeshDim; j++) {
                    pV[i * (_gMeshDim + 1) + j].x = (float) j;
                    pV[i * (_gMeshDim + 1) + j].y = (float) i;
                }
            }

//        D3D11_BUFFER_DESC vb_desc;
//        vb_desc.ByteWidth = num_verts * sizeof(Vector2);
//        vb_desc.Usage = D3D11_USAGE_IMMUTABLE;
//        vb_desc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
//        vb_desc.CPUAccessFlags = 0;
//        vb_desc.MiscFlags = 0;
//        vb_desc.StructureByteStride = sizeof(Vector2);
//
//        D3D11_SUBRESOURCE_DATA init_data;
//        init_data.pSysMem = pV;
//        init_data.SysMemPitch = 0;
//        init_data.SysMemSlicePitch = 0;
//
//        SAFE_RELEASE(g_pMeshVB);
//        pd3dDevice->CreateBuffer(&vb_desc, &init_data, &g_pMeshVB);
//        assert(g_pMeshVB);
//
//        SAFE_DELETE_ARRAY(pV);


            // --------------------------------- Index Buffer -------------------------------
            // The index numbers for all mesh LODs (up to 256x256)
            int[] indexSizeLookup = {0, 0, 4284, 18828, 69444, 254412, 956916, 3689820, 14464836};

            //memset(&g_mesh_patterns[0][0][0][0][0], 0, sizeof(g_mesh_patterns));
            _gMeshPatterns[0, 0, 0, 0, 0] = new QuadRenderParam();

            _gLods = 0;
            for (i = _gMeshDim; i > 1; i >>= 1)
                _gLods++;

            // Generate patch meshes. Each patch contains two parts: the inner mesh which is a regular
            // grids in a triangle strip. The boundary mesh is constructed w.r.t. the edge degrees to
            // meet water-tight requirement.
            int[] indexArray = new int[indexSizeLookup[_gLods]];

            int offset = 0;
            int levelSize = _gMeshDim;

            // Enumerate patterns
            for (int level = 0; level <= _gLods - 2; level++) {
                int leftDegree = levelSize;

                for (int leftType = 0; leftType < 3; leftType++) {
                    int rightDegree = levelSize;

                    for (int rightType = 0; rightType < 3; rightType++) {
                        int bottomDegree = levelSize;

                        for (int bottomType = 0; bottomType < 3; bottomType++) {
                            int topDegree = levelSize;

                            for (int topType = 0; topType < 3; topType++) {
                                QuadRenderParam pattern =
                                    _gMeshPatterns[level, leftType, rightType, bottomType, topType];

                                // Inner mesh (triangle strip)
                                Rect innerRect;
                                innerRect.Left = (leftDegree == levelSize) ? 0 : 1;
                                innerRect.Right = (rightDegree == levelSize) ? levelSize : levelSize - 1;
                                innerRect.Bottom = (bottomDegree == levelSize) ? 0 : 1;
                                innerRect.Top = (topDegree == levelSize) ? levelSize : levelSize - 1;

                                int numNewIndices = GenerateInnerMesh(innerRect, indexArray, offset);

                                pattern.InnerStartIndex = offset;
                                pattern.NumInnerVerts = (levelSize + 1) * (levelSize + 1);
                                pattern.NumInnerFaces = numNewIndices - 2;
                                offset += numNewIndices;

                                // Boundary mesh (triangle list)
                                int lDegree = (leftDegree == levelSize) ? 0 : leftDegree;
                                int rDegree = (rightDegree == levelSize) ? 0 : rightDegree;
                                int bDegree = (bottomDegree == levelSize) ? 0 : bottomDegree;
                                int tDegree = (topDegree == levelSize) ? 0 : topDegree;

                                Rect outerRect = new Rect(0, levelSize, levelSize, 0);
                                numNewIndices = GenerateBoundaryMesh(lDegree, rDegree, bDegree, tDegree, outerRect,
                                    indexArray, offset);

                                pattern.BoundaryStartIndex = offset;
                                pattern.NumBoundaryVerts = (levelSize + 1) * (levelSize + 1);
                                pattern.NumBoundaryFaces = numNewIndices / 3;
                                offset += numNewIndices;

                                topDegree /= 2;
                            }
                            bottomDegree /= 2;
                        }
                        rightDegree /= 2;
                    }
                    leftDegree /= 2;
                }
                levelSize /= 2;
            }

            Debug.Log("?????");
            //assert(offset == indexSizeLookup[g_Lods]);

//        D3D11_BUFFER_DESC ib_desc;
//        ib_desc.ByteWidth = indexSizeLookup[g_Lods] * sizeof(DWORD);
//        ib_desc.Usage = D3D11_USAGE_IMMUTABLE;
//        ib_desc.BindFlags = D3D11_BIND_INDEX_BUFFER;
//        ib_desc.CPUAccessFlags = 0;
//        ib_desc.MiscFlags = 0;
//        ib_desc.StructureByteStride = sizeof(DWORD);
//
//        init_data.pSysMem = index_array;
//
//        SAFE_RELEASE(g_pMeshIB);
//        pd3dDevice->CreateBuffer(&ib_desc, &init_data, &g_pMeshIB);
//        assert(g_pMeshIB);
//
//        SAFE_DELETE_ARRAY(index_array);
        }
    }
}