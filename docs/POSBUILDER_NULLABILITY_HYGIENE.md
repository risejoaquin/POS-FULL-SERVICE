# PosBuilder Nullability Hygiene

## PHASE 7F

This document defines the targeted PosBuilder nullability hygiene pass.

## Scope

PosBuilder nullability hygiene documented.
CS8618 non-nullable initialization hygiene documented.
CS8622 event handler nullability hygiene documented.
CS8603 converter return nullability hygiene documented.
CS8600 and CS8601 possible null assignment hygiene documented.

## Targeted areas

App.xaml.cs handler sender nullable compatibility applied.
ConfigModel LicenseKey initialized.
WizardViewModel step title fields initialized.
MainWindow StepIndicator fields initialized.
MainWindow provisioning response null guards applied.
Converters ConvertBack null guard applied.
ColorPickerControl event and palette model nullability hygiene applied.
FileBrowserControl event nullability hygiene applied.
NotificationService brush conversion null guard applied.

## Safety boundaries

PosBuilder UI only remediation scope documented.
No checkout behavior change.
No inventory mutation.
No production sync enablement.
No public API behavior change.
No schema change.
No migrations.

## Operator-safe message

PosBuilder nullability hygiene applied only to initialization, event compatibility and safe UI conversion boundaries.
