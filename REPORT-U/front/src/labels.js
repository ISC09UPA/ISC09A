// Labels con acentos para los enums que viajan como strings de la API.
import { CATEGORIES, TYPE_OPTIONS, IDENTITY_OPTIONS, DISPLAY_NAME_OPTIONS } from './constants';

const byValue = (options) => Object.fromEntries(options.map((o) => [o.value, o.label]));
const byLabel = (options) => Object.fromEntries(options.map((o) => [o.label, o.value]));

const CATEGORY_LABELS = byValue(CATEGORIES);
const CATEGORY_VALUES = byLabel(CATEGORIES);
const TYPE_LABELS = byValue(TYPE_OPTIONS);
const IDENTITY_LABELS = byValue(IDENTITY_OPTIONS);
const DISPLAY_NAME_LABELS = byValue(DISPLAY_NAME_OPTIONS);
const DISPLAY_NAME_VALUES = byLabel(DISPLAY_NAME_OPTIONS);

export const categoryLabel = (value) => CATEGORY_LABELS[value] || value || '';
export const categoryValue = (label) => CATEGORY_VALUES[label] || label || '';
export const typeLabel = (value) => TYPE_LABELS[value] || value || '';
export const identityLabel = (value) => IDENTITY_LABELS[value] || value || '';
export const displayNameLabel = (value) => DISPLAY_NAME_LABELS[value] || value || '';
export const displayNameValue = (label) => DISPLAY_NAME_VALUES[label] || label || '';
