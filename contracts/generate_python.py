"""Generate the Python contract models from contracts/*.schema.json.

Run from the repo root:
    uv run python contracts/generate_python.py          # write the models
    uv run python contracts/generate_python.py --check  # exit 1 if a model is out of date
"""

import argparse
import sys
from pathlib import Path

from datamodel_code_generator import (
    DataModelType,
    Formatter,
    InputFileType,
    PythonVersion,
    generate,
)

REPO_ROOT = Path(__file__).resolve().parent.parent
CONTRACTS_DIR = REPO_ROOT / "contracts"
OUTPUT_DIR = REPO_ROOT / "edge" / "shared" / "src" / "cell_shared" / "contracts"
SCHEMA_SUFFIX = ".schema.json"


def module_path(schema_path: Path) -> Path:
    """Map contracts/<name>.schema.json to cell_shared/contracts/<name_with_underscores>.py."""
    stem = schema_path.name.removesuffix(SCHEMA_SUFFIX).replace("-", "_")
    return OUTPUT_DIR / f"{stem}.py"


def render(schema_path: Path) -> str:
    """Return the generated module text for one schema."""
    header = (
        f"# Generated from contracts/{schema_path.name} by contracts/generate_python.py.\n"
        "# Do not edit. Change the schema and run the generator again."
    )
    result = generate(
        schema_path,
        input_file_type=InputFileType.JsonSchema,
        output_model_type=DataModelType.PydanticV2BaseModel,
        target_python_version=PythonVersion.PY_312,
        use_annotated=True,
        field_constraints=True,
        use_standard_collections=True,
        use_union_operator=True,
        use_double_quotes=True,
        use_schema_description=True,
        use_field_description=True,
        disable_timestamp=True,
        custom_file_header=header,
        formatters=[Formatter.RUFF_FORMAT, Formatter.RUFF_CHECK],
        settings_path=REPO_ROOT / "pyproject.toml",
    )
    if not isinstance(result, str):
        msg = f"generator returned {type(result).__name__}, expected one module for {schema_path}"
        raise TypeError(msg)
    # The generator API drops the final newline that ruff format and end-of-file-fixer require.
    return result.rstrip("\n") + "\n"


def schemas() -> list[Path]:
    return sorted(CONTRACTS_DIR.glob(f"*{SCHEMA_SUFFIX}"))


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Generate the Python contract models.")
    parser.add_argument("--check", action="store_true", help="fail if a model is out of date")
    args = parser.parse_args(argv)

    stale: list[Path] = []
    for schema_path in schemas():
        target = module_path(schema_path)
        text = render(schema_path)
        current = target.read_text(encoding="utf-8") if target.exists() else None
        if current == text:
            continue
        if args.check:
            stale.append(target)
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(text, encoding="utf-8", newline="\n")
            print(f"wrote {target.relative_to(REPO_ROOT).as_posix()}")

    for target in stale:
        print(f"out of date: {target.relative_to(REPO_ROOT).as_posix()}", file=sys.stderr)
    return 1 if stale else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
