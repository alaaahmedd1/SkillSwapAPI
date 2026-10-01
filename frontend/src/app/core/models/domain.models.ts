// Mirrors backend DTOs for profiles, swap requests, chat and live sessions.
// Enums serialize as integers (see backend Domain enums).

// ── Users / profiles ────────────────────────────────────────────────────────
export const SkillType = { Offered: 1, Seeking: 2 } as const;
export const ProficiencyLevel = { Beginner: 1, Intermediate: 2, Expert: 3 } as const;

export interface UserSkillDto {
  id: string; // user-skill row id (needed for DELETE)
  skillId: string;
  skillName: string;
  categoryName: string;
  type: number;
  proficiencyLevel: number;
  yearsOfExperience: number | null;
}

export interface ProfileDto {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  averageRating: number;
  totalReviewsCount: number;
  isActive: boolean;
  createdAtUtc: string;
  userSkills: UserSkillDto[];
  title: string | null;
  bio: string | null;
  city: string | null;
  country: string | null;
  timeZone: string | null;
  openForInstantSwaps: boolean;
  onlineOnly: boolean;
  autoMatchBarterRequests: boolean;
}

export type TimeBlock = { Morning: 1; Afternoon: 2; Evening: 3 }[keyof { Morning: 1; Afternoon: 2; Evening: 3 }];

export interface AvailabilitySlotDto {
  dayOfWeek: number; // 0 = Sunday (JS/ISO convention)
  timeBlock: number; // 1 Morning, 2 Afternoon, 3 Evening
}

export interface UserSkillSummaryDto {
  skillId: string;
  skillName: string;
  categoryName: string;
  type: number;
  proficiencyLevel: number;
}

export interface UserSearchResultDto {
  userId: string;
  firstName: string;
  lastName: string;
  averageRating: number;
  totalReviewsCount: number;
  skills: UserSkillSummaryDto[];
}

export interface ReviewDto {
  id: string;
  swapRequestId: string;
  reviewerId: string;
  reviewerFirstName: string;
  reviewerLastName: string;
  revieweeId: string;
  rating: number;
  comment: string | null;
  createdAtUtc: string;
}

export interface UserBadgeDto {
  badgeId: number;
  name: string;
  description: string;
  iconUrl: string | null;
  awardCount: number;
  lastAwardedAtUtc: string;
}

// ── Swap requests ───────────────────────────────────────────────────────────
export const SwapRequestStatus = {
  Pending: 1,
  Accepted: 2,
  Rejected: 3,
  Cancelled: 4,
  Completed: 5,
} as const;

export const ProposalStatus = { Proposed: 1, Accepted: 2, Rejected: 3 } as const;

export interface SwapRequestSkillDto {
  skillId: string;
  skillName: string;
  categoryName: string;
}

export interface SwapRequestDto {
  id: string;
  requesterId: string;
  receiverId: string;
  requesterFirstName: string;
  requesterLastName: string;
  receiverFirstName: string;
  receiverLastName: string;
  offeredSkill: SwapRequestSkillDto;
  requestedSkill: SwapRequestSkillDto;
  status: number;
  isRequesterConfirmed: boolean;
  isReceiverConfirmed: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  conversationId: string | null;
}

export interface SessionProposalDto {
  id: string;
  swapRequestId: string;
  proposerId: string;
  scheduledDate: string; // "yyyy-MM-dd"
  startTime: string; // "HH:mm:ss"
  endTime: string;
  durationMinutes: number;
  status: number;
  createdAtUtc: string;
}

export interface SwapRequestDetailsDto extends SwapRequestDto {
  proposedScheduleDetails: string | null;
  sessionProposals: SessionProposalDto[];
}

export interface CreateSwapRequestPayload {
  receiverId: string;
  offeredSkillId: string;
  requestedSkillId: string;
  proposedScheduleDetails?: string | null;
}

// ── Wallet & payments ───────────────────────────────────────────────────────
export interface WalletBalanceDto {
  balanceMinutes: number;
  totalEarnedMinutes: number;
  totalSpentMinutes: number;
}

export type WalletTransactionType = 'Earned' | 'Spent' | 'Purchased' | 'Refunded';

export interface WalletTransactionDto {
  id: string;
  referenceCode: string;
  title: string;
  transactionType: WalletTransactionType;
  amountMinutes: number;
  runningBalanceMinutes: number;
  swapRequestId: string | null;
  partnerUserId: string | null;
  createdAtUtc: string;
}

export interface CreditPackageDto {
  id: string;
  name: string;
  description: string;
  creditsCount: number;
  price: number;
  currency: string;
}

export interface CheckoutResponseDto {
  orderId: string;
  clientSecret: string;
  amount: number;
  currency: string;
}

export interface PaymentConfigDto {
  publishableKey: string;
}

// ── Chat ────────────────────────────────────────────────────────────────────
export interface MessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  isRead: boolean;
  sentAtUtc: string;
}

// ── Live sessions ───────────────────────────────────────────────────────────
export const LiveSessionStatus = { Waiting: 1, InProgress: 2, Ended: 3 } as const;

export interface LiveSessionRoomDto {
  id: string;
  swapRequestId: string;
  roomToken: string;
  scheduledStartTime: string;
  actualStartTime: string | null;
  actualEndTime: string | null;
  durationSeconds: number;
  status: number;
}

export interface LiveSessionTimerDto {
  startedAtUtc: string | null;
  elapsedSeconds: number;
}

export interface RtcSessionDescriptionDto {
  type: string;
  sdp: string;
}

export interface RtcIceCandidateDto {
  candidate: string;
  sdpMid: string | null;
  sdpMLineIndex: number | null;
}
