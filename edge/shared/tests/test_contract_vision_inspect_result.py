"""Contract tests for the vision.inspect result (architecture §7.2)."""

import copy
import importlib.util
import json
from datetime import datetime
from pathlib import Path
from types import ModuleType
from typing import Any

import pytest
from jsonschema import Draft202012Validator
from pydantic import ValidationError

from cell_shared.contracts.vision_inspect_result import VisionInspectResult

REPO_ROOT = Path(__file__).resolve().parents[3]
SCHEMA_PATH = REPO_ROOT / "contracts" / "vision-inspect-result.schema.json"
EXAMPLE_PATH = REPO_ROOT / "contracts" / "examples" / "vision-inspect-result.json"
GENERATOR_PATH = REPO_ROOT / "contracts" / "generate_python.py"


def load_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


@pytest.fixture(scope="module")
def schema() -> dict[str, Any]:
    return load_json(SCHEMA_PATH)


@pytest.fixture(scope="module")
def validator(schema: dict[str, Any]) -> Draft202012Validator:
    return Draft202012Validator(schema, format_checker=Draft202012Validator.FORMAT_CHECKER)


@pytest.fixture
def example() -> dict[str, Any]:
    return load_json(EXAMPLE_PATH)


def test_schema_is_valid_draft_2020_12(schema: dict[str, Any]) -> None:
    Draft202012Validator.check_schema(schema)
    assert schema["$id"] == "urn:foundry-in-private:contracts:vision-inspect-result:1.0.0"


def test_example_is_valid(validator: Draft202012Validator, example: dict[str, Any]) -> None:
    assert list(validator.iter_errors(example)) == []


def test_format_checks_are_active(validator: Draft202012Validator, example: dict[str, Any]) -> None:
    example["captured_at"] = "2026-13-40T99:00:00Z"
    assert not validator.is_valid(example)


def _extra_field(d: dict[str, Any]) -> None:
    d["unexpected"] = 1


def _nested_extra_field(d: dict[str, Any]) -> None:
    d["detections"][0]["unexpected"] = 1


def _confidence_above_one(d: dict[str, Any]) -> None:
    d["detections"][0]["confidence"] = 1.5


def _bbox_of_three(d: dict[str, Any]) -> None:
    d["detections"][0]["bbox_xyxy"] = [1, 2, 3]


def _no_inspection_id(d: dict[str, Any]) -> None:
    del d["inspection_id"]


def _captured_at_not_utc(d: dict[str, Any]) -> None:
    d["captured_at"] = "2026-11-20T22:14:07.412+01:00"


BAD_INPUTS = {
    "extra_field": _extra_field,
    "nested_extra_field": _nested_extra_field,
    "confidence_above_one": _confidence_above_one,
    "bbox_of_three": _bbox_of_three,
    "no_inspection_id": _no_inspection_id,
    "captured_at_not_utc": _captured_at_not_utc,
}


@pytest.mark.parametrize("break_it", BAD_INPUTS.values(), ids=BAD_INPUTS.keys())
def test_schema_refuses_bad_input(
    validator: Draft202012Validator, example: dict[str, Any], break_it: Any
) -> None:
    bad = copy.deepcopy(example)
    break_it(bad)
    assert not validator.is_valid(bad)


# The Python model does not see the "Z$" pattern on captured_at (pydantic drops it, OD7).
MODEL_BAD_INPUTS = {k: v for k, v in BAD_INPUTS.items() if k != "captured_at_not_utc"}


def load_generator() -> ModuleType:
    spec = importlib.util.spec_from_file_location("generate_python", GENERATOR_PATH)
    assert spec is not None
    assert spec.loader is not None
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def test_python_model_is_current_generator_output() -> None:
    generator = load_generator()
    target: Path = generator.module_path(SCHEMA_PATH)
    with pytest.warns(UserWarning, match="pattern on AwareDatetime"):  # OD7: known constraint drop
        rendered = generator.render(SCHEMA_PATH)
    assert target.read_text(encoding="utf-8") == rendered


def test_example_round_trips_through_the_python_model(
    validator: Draft202012Validator, example: dict[str, Any]
) -> None:
    model = VisionInspectResult.model_validate(example)
    dumped = json.loads(model.model_dump_json())

    assert list(validator.iter_errors(dumped)) == []
    assert VisionInspectResult.model_validate(dumped) == model
    assert dumped["inspection_id"] == example["inspection_id"]
    assert model.captured_at == datetime.fromisoformat(example["captured_at"])
    assert [d.confidence for d in model.detections] == [0.87, 0.41]


@pytest.mark.parametrize("break_it", MODEL_BAD_INPUTS.values(), ids=MODEL_BAD_INPUTS.keys())
def test_python_model_refuses_bad_input(example: dict[str, Any], break_it: Any) -> None:
    bad = copy.deepcopy(example)
    break_it(bad)
    with pytest.raises(ValidationError):
        VisionInspectResult.model_validate(bad)


def test_python_model_refuses_a_naive_capture_time(example: dict[str, Any]) -> None:
    example["captured_at"] = "2026-11-20T21:14:07.412"
    with pytest.raises(ValidationError):
        VisionInspectResult.model_validate(example)
