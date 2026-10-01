// Mirrors the backend SkillSwapAPI DTOs (camelCase JSON).
// See src/SkillSwapAPI.Application — enums serialize as integers.

export interface UserDto {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  token: string | null;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresOnUtc: string;
  user: UserDto;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresOnUtc: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PublicListingDto {
  id: string;
  displayName: string;
  title: string;
  country: string;
  skillsOffered: string[];
  skillsWanted: string[];
  city: string;
  averageRating: number;
  totalReviewsCount: number;
}

export interface SkillDto {
  id: string;
  name: string;
  description: string;
}

export interface SkillCatalogItemDto {
  id: number;
  name: string;
  description: string;
  skills: SkillDto[];
}

// Backend error envelope: { code, message, errors? } — errors only on 400 validation,
// each item's `code` is the failing property name (FluentValidation PropertyName).
export interface ApiErrorItem {
  code: string;
  description: string;
  type?: number;
}

export interface ApiErrorBody {
  code?: string;
  message?: string;
  errors?: ApiErrorItem[];
  title?: string; // RFC 7807 ProblemDetails for unhandled exceptions
}
