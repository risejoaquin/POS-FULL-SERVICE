with open('.github/workflows/build-release.yml', 'r') as f:
    c = f.read()

new_steps = """      - name: Restore Dependencies
        run: dotnet restore Pos.sln

      - name: Build Solution
        run: dotnet build Pos.sln -c Release --no-restore

      - name: Run Unit Tests
        run: dotnet test PosDomain.Tests -c Release --no-build
        continue-on-error: true

      - name: Run Integration Tests
        run: dotnet test PosCore.Tests -c Release --no-build
        continue-on-error: true

      - name: Publish App"""

c = c.replace("- name: Publish App", new_steps)

with open('.github/workflows/build-release.yml', 'w') as f:
    f.write(c)
