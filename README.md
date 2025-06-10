# Known Issues
- ~~There appears to have arisen some form of issue with one or both of the following, causing verts near the floor to pop out of existence:~~
   - ~~The depth stencil buffer~~ (it was this; incorrect configuration of clipping planes)
   - ~~My PS1 shader's manipulation of clip space~~
- The walls in the "test chamber" don't light evenly
   - A vertex-lit mesh can only receive lighting from 4 lights at once; this is an elementary engine limitation
   - The solution is to reduce the number of lights, _or_ spread them out _and_ merge the planes composing the walls and floor of the room such that lights can't interfere with one another at the edges of the meshes they light up
      - This could also be achieved by adding a `VertexLM` option to the shader, allowing static surfaces to be lightmapped during a baked lighting pass
