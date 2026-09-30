import { reactivateMember } from "../../api/access.api";
import { useOptimisticMemberMutation } from "./useOptimisticMemberMutation";

export const useReactivateMember = () => useOptimisticMemberMutation(reactivateMember, "Approved", "Couldn't reactivate this member.");
